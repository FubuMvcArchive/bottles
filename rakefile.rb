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
	
	sln.defaults = [:ilrepack, :integration_test]
	sln.ci_steps = [:ilrepack]
	
	
	sln.compile_step :compile_console, 'src/Bottles.Console/Bottles.Console.csproj'
	sln.compile_step :compile_bottle_project, 'bottles-staging/BottleProject.csproj'

	sln.precompile = [:compile_console, :bottle_assembly_package]
	
	sln.integration_test = ['Bottles.IntegrationTesting']
end

desc "**Mono**, compiles, merges and runs unit tests"
task :mono_ci => [:compile, :ilrepack, :unit_test]

desc "does the assembly bottling of the AssemblyPackage test project"
task :bottle_assembly_package => [:compile_bottle_project] do
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
	packer.merge :lib => dir, :refs => [assembly, 'Ionic.Zip.dll']
end

def bottles(args)
  sh Platform.runtime("src/Bottles.Console/bin/#{@solution.compilemode}/BottleRunner.exe #{args}")
end

