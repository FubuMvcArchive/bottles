begin
  require 'bundler/setup'
  require 'fuburake'
rescue LoadError
  puts 'Bundler and all the gems need to be installed prior to running this rake script. Installing...'
  system("gem install bundler --source http://rubygems.org")
  sh 'bundle install'
  system("bundle exec rake", *ARGV)
  exit 0
end


@solution = FubuRake::Solution.new do |sln|
	sln.compile = {
		:solutionfile => 'src/Bottles.sln'
	}
				 
	sln.assembly_info = {
		:product_name => "Bottles",
		:copyright => 'Copyright 2008-2013 Jeremy D. Miller, Dru Sellers, Joshua Flanagan, et al. All rights reserved.'
	}
	
	sln.options = {
		:unit_test_projects => ['Bottles.Tests']
	}
	
	sln.ripple_enabled = true
	sln.fubudocs_enabled = true
	
	# TODO -- put the integration testing back into the build.  Has file cleanup problems now
	sln.defaults = [:ilrepack]
	sln.ci_steps = [:ilrepack, :archive_gem]
	
	
	sln.compile_step :compile_console, 'src/Bottles.Console/Bottles.Console.csproj'
	sln.compile_step :compile_bottle_project, 'bottles-staging/BottleProject.csproj'

	sln.precompile = [:compile_console, :bottle_assembly_package]
	
	sln.integration_test = ['Bottles.IntegrationTesting']
end

desc "**Mono**, compiles, merges and runs unit tests"
task :mono_ci => [:compile, :ilrepack, :unit_test]

desc "does the assembly bottling of the AssemblyPackage test project"
task :bottle_assembly_package do
  bottles "assembly-pak src/AssemblyPackage"
end

desc "Merge dotnetzip assembly into Bottles projects"
task :ilrepack do
  merge_ionic("src/Bottles/bin/#{@solution.compilemode}", 'Bottles.dll')
end

require_relative 'ILRepack'

def merge_ionic(dir, assembly)
	output = File.join(dir, assembly)
	packer = ILRepack.new :out => output, :lib => dir
	packer.merge :lib => dir, :refs => [assembly, 'Ionic.Zip.dll'], :clrversion => @solution.options[:clrversion]
end

def bottles(args)
  sh Platform.runtime("src/Bottles.Console/bin/#{@solution.compilemode}/BottleRunner.exe #{args}", @solution.options[:clrversion])
end

desc "Replaces the existing installed gem with the new version for local testing"
task :local_gem => [:create_gem] do
	sh 'gem uninstall bottles'
	Dir.chdir 'pkg' do
	    sh 'gem install bottles'
    end
end

desc "Moves the gem to the archive folder"
task :archive_gem => [:create_gem] do
	copyOutputFiles "pkg", "*.gem", "artifacts"
end

desc "Outputs the command line usage"
task :dump_usages => [:compile] do
  bottles 'dump-usages bottles src/Bottles.Docs/bottles.cli.xml'
end

desc "Creates the gem for BottleRunner.exe"
task :create_gem => [:compile, :ilrepack] do
    require "rubygems/package"
	cleanDirectory 'bin';	
	cleanDirectory 'pkg'
	

	copyOutputFiles "src/Bottles.Console/bin/#{@solution.compilemode}", '*.dll', 'bin'
	copyOutputFiles "src/Bottles.Console/bin/#{@solution.compilemode}", '*BottleRunner.exe', 'bin/bottles.exe'
	
	FileUtils.copy 'bottles', 'bin'
	
	# letting the file system catch up, otherwise you occasionally
	# get a gem w/ no binaries, and it always happens on CI
	waitfor {exists?('bin/bottles.exe')}
	waitfor {exists?('bin/Bottles.dll')}
	waitfor {exists?('bin/FubuCore.dll')}
	
	Dir.foreach('bin') do |item|
	  puts item
	end

    onthefly = Gem::Specification.new do |s|
        s.name        = 'bottles'
        s.version     = @solution.build_number
        s.files =  Dir.glob("bin/**/*").to_a
        s.files += Dir['lib/*.rb']
        s.bindir = 'bin'
        s.executables << 'bottles'

        s.license = 'Apache 2'

        s.summary     = 'Command line tools for using Bottles'
        s.description = 'Shared libraries for runtime and deployment packaging of .Net'

        s.authors           = ['Jeremy D. Miller', 'Josh Arnold', 'Chad Myers', 'Joshua Flanagan']
        s.email             = 'fubumvc-devel@googlegroups.com'
        s.homepage          = 'http://fubu-project.org'
        s.rubyforge_project = 'bottles'
    end     
    puts "ON THE FLY SPEC FILES"
    puts onthefly.files
    puts "=========="

    Gem::Package::build onthefly, true

	copyOutputFiles ".", "*.gem", "pkg"
end
