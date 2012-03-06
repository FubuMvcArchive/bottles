include FileTest
require 'albacore'
load "VERSION.txt"

COMPILE_TARGET = ENV['config'].nil? ? "Debug" : ENV['config']
CLR_TOOLS_VERSION = "v4.0.30319"
RESULTS_DIR = "results"
PRODUCT = "FubuMVC"
COPYRIGHT = 'Copyright 2010-2011 Jeremy D. Miller, Dru Sellers, et al. All rights reserved.';
COMMON_ASSEMBLY_INFO = 'src/CommonAssemblyInfo.cs';

buildsupportfiles = Dir["#{File.dirname(__FILE__)}/buildsupport/*.rb"]
raise "Run `git submodule update --init` to populate your buildsupport folder." unless buildsupportfiles.any?
buildsupportfiles.each { |ext| load ext }

@teamcity_build_id = "bt392"
tc_build_number = ENV["BUILD_NUMBER"]
build_revision = tc_build_number || Time.new.strftime('5%H%M')
BUILD_NUMBER = "#{BUILD_VERSION}.#{build_revision}"
ARTIFACTS = File.expand_path("artifacts")

props = { :stage => File.expand_path("build"), :artifacts => ARTIFACTS }

desc "**Default**, compiles, merges and runs tests"
task :default => [:compile, :ilrepack, :create_deployer_bottles, :unit_test]

desc "Creates and publishes the nuget files for the current code"
task :local_nuget_push => [:compile, :ilrepack, :create_deployer_bottles, "nuget:build", "nuget:push"]

desc "Target used for the CI server"
task :ci => [:update_all_dependencies, :default,:package,:create_deployer_bottles,:history]

desc "Update the version information for the build"
assemblyinfo :version do |asm|
  asm_version = BUILD_VERSION + ".0"
  
  begin
    commit = `git log -1 --pretty=format:%H`
  rescue
    commit = "git unavailable"
  end
  puts "##teamcity[buildNumber '#{BUILD_NUMBER}']" unless tc_build_number.nil?
  puts "Version: #{BUILD_NUMBER}" if tc_build_number.nil?
  asm.trademark = commit
  asm.product_name = PRODUCT
  asm.description = BUILD_NUMBER
  asm.version = asm_version
  asm.file_version = BUILD_NUMBER
  asm.custom_attributes :AssemblyInformationalVersion => asm_version
  asm.copyright = COPYRIGHT
  asm.output_file = COMMON_ASSEMBLY_INFO
end

desc "Prepares the working directory for a new build"
task :clean do
	#TODO: do any other tasks required to clean/prepare the working directory
	FileUtils.rm_rf props[:stage]
    # work around nasty latency issue where folder still exists for a short while after it is removed
    waitfor { !exists?(props[:stage]) }
	Dir.mkdir props[:stage]
    
	Dir.mkdir props[:artifacts] unless exists?(props[:artifacts])
end

def waitfor(&block)
  checks = 0
  until block.call || checks >10 
    sleep 0.5
    checks += 1
  end
  raise 'waitfor timeout expired' if checks > 10
end

desc "Compiles the app"
task :compile => [:restore_if_missing, :clean, :version] do
  MSBuildRunner.compile :compilemode => COMPILE_TARGET, :solutionfile => 'src/Bottles.Console/Bottles.Console.csproj', :clrversion => CLR_TOOLS_VERSION
  bottles "assembly-pak src/AssemblyPackage"
  MSBuildRunner.compile :compilemode => COMPILE_TARGET, :solutionfile => 'src/Bottles.sln', :clrversion => CLR_TOOLS_VERSION
  
  sleep 1
  puts 'Trying to copy files from bin directories to the build directory'
  #copyOutputFiles "src/Milkman.Deployers.Iis/bin/#{COMPILE_TARGET}", "*.{dll,pdb}", props[:stage]  
  #copyOutputFiles "src/Milkman.Deployers.Topshelf/bin/#{COMPILE_TARGET}", "*.{dll,pdb}", props[:stage]
  #copyOutputFiles "src/Bottles.Host/bin/#{COMPILE_TARGET}", "*.{dll,pdb,exe}", props[:stage]
  #copyOutputFiles "src/Bottles.Console/bin/#{COMPILE_TARGET}", "*.{dll,pdb,exe,config}", props[:stage]
end

def copyOutputFiles(fromDir, filePattern, outDir)
  Dir.glob(File.join(fromDir, filePattern)){|file| 		
	copy(file, outDir) if File.file?(file)
  } 
end

desc "Runs unit tests"
task :test => [:unit_test]

desc "Runs unit tests"
task :unit_test => :compile do
  runner = NUnitRunner.new :compilemode => COMPILE_TARGET, :source => 'src', :platform => 'x86'
  runner.executeTests ['Bottles.Tests']
end

desc "Runs the StoryTeller suite of end to end tests.  IIS must be running first"
task :storyteller => [:compile] do
  echo 'Not ready yet'
  # sh "lib/storyteller/StoryTellerRunner Storyteller.xml output/st-results.htm"
end

desc "ZIPs up the build results"
zip :package do |zip|
	zip.directories_to_zip = [props[:stage]]
	zip.output_file = 'bottles.zip'
	zip.output_path = [props[:artifacts]]
end

desc "Creates the deployer bottle files"
task :create_deployer_bottles => :compile do
  bottles "create-pak src/Bottles.Console build/bottles.zip -target #{COMPILE_TARGET}"
  bottles "create-pak src/Bottles.Host build/topshelf-deployers.zip -target #{COMPILE_TARGET}"
  bottles "create-pak src/Milkman.Deployers.Iis build/iis-deployers.zip -target #{COMPILE_TARGET}"
end

desc "Merge dotnetzip assembly into Bottles projects"
task :ilrepack do
  merge_ionic("src/Bottles/bin/#{COMPILE_TARGET}", 'Bottles.dll')
  merge_ionic("src/Milkman/bin/#{COMPILE_TARGET}", 'Milkman.dll')
end

def merge_ionic(dir, assembly)
	output = File.join(dir, assembly)
	packer = ILRepack.new :out => output, :lib => dir
	packer.merge :lib => dir, :refs => [assembly, 'Ionic.Zip.dll']
end

def bottles(args)
  sh Platform.runtime("src/Bottles.Console/bin/#{COMPILE_TARGET}/BottleRunner.exe #{args}")
end
