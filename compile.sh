#!/bin/bash

# Use this script to compile project file from command line in Unix-like systems.
# Usage:
# $ ./compile.sh <project name> [-s]
#
# Example: ./compile.sh MainFrame
# if -s is set, the template compilation is ignored

cwd=$(pwd)

if [ "$#" -eq 0 ]; then
    echo "Illegal parameters. Usage: ./compile.sh <project name> [-a] [-p <project path>]"
    exit 1
fi

# The directory where implementations are locaded

implementation_path="$cwd/.."

for (( i=2; i <= "$#"; i++ )); do
	if [ ${!i} == "-p" ]; then
		implementations_index=$((i+1)) 
		if [ -z ${!implementations_index} ]; then
			echo "No implementation path for parameter -p specified"
			exit 1
		fi
		implementation_path="$cwd/../${!implementations_index}"
	fi
done

if [ ! -d $implementation_path ]; then
    echo "Project path '$implementation_path' not found."
    exit 1
fi

echo "Using implementation base path: $implementation_path"

T4_compiler_path="/usr/lib/monodevelop/AddIns/MonoDevelop.TextTemplating/TextTransform.exe"
nuget_path="/usr/local/bin/nuget.exe"
core_configuration_path="Src/Core/Settings"
T4_compiler_command="$T4_compiler_path -I $core_configuration_path"
dir_core="$cwd/Src/Core"
dir_audio="$cwd/Src/Audio"
dir_video="$cwd/Src/Video"
dir_scripting="$cwd/Src/Scripting"
dir_push="$cwd/Src/PushNotifications"
dir_gpio="$cwd/Src/GPIO"
dir_common="$cwd/Src/Common"

core_configuration_template="$cwd/$core_configuration_path/CoreConfigurationTemplate.tt"
core_configuration_output="$cwd/$core_configuration_path/CoreConfigurationTemplate.cs"
audio_configuration_template="$dir_audio/AudioConfigurationTemplate.tt"
audio_configuration_output="$dir_audio/AudioConfigurationTemplate.cs"
video_configuration_template="$dir_video/VideoConfigurationTemplate.tt"
video_configuration_output="$dir_video/VideoConfigurationTemplate.cs"
gpio_configuration_template="$dir_gpio/GPIOConfigurationTemplate.tt"
gpio_configuration_output="$dir_gpio/GPIOConfigurationTemplate.cs"
common_configuration_template="$dir_common/CommonConfigurationTemplate.tt"
common_configuration_output="$dir_common/CommonConfigurationTemplate.cs"
script_configuration_template="$dir_scripting/ScriptingConfigurationTemplate.tt"
script_configuration_output="$dir_scripting/ScriptingConfigurationTemplate.cs"
push_configuration_template="$dir_push/PushNotificationsConfigurationTemplate.tt"
push_configuration_output="$dir_push/PushNotificationsConfigurationTemplate.cs"

template_file="$implementation_path/$1/$1/$1ConfigurationTemplate.tt"
project_file="$implementation_path/$1/$1.sln"

if [ ! -f $nuget_path ]; then
	echo "Nuget package manager not found at: $nuget_path."
    exit 1
fi

package_paths=($dir_core $dir_audio $dir_video $dir_scripting $dir_common $dir_gpio $dir_push)

echo "$(tput bold)--Installing packages--$(tput sgr 0)"

for i in "${!package_paths[@]}"; do
	package_path=${package_paths[$i]}
	package_dir="$package_path/packages.config"
	if [ -f $package_dir ]; then
		mono "$nuget_path" install "$package_dir" -OutputDirectory "$implementation_path/$1/packages" | exit 1
	fi
done

if [ ! -f $T4_compiler_path ]; then
    echo "T4 compiler not found at '$T4_compiler_path'. Install monodevelop. (apt-get install monodevelop in Debian/Ubuntu)."
    exit 1
fi

if [ ! -f $project_file ]; then
    echo "Project file: $project_file does not exist."
    exit 1
fi

if [ ! -f $template_file ]; then
    echo "T4 configuration file: $template_file does not exist. Please follow naming conventions when creating configuration templates (<project dir>/<project name>ConfigurationTemplate.tt)."
    exit 1
fi

function compileT4 {

	echo "Compiling T4 template '$1'..."
	eval "$T4_compiler_command $1 -o $2"

	if [ ! $? -eq 0 ];
	then
        	echo "Unable to compile T4 template '$1'."
        	exit 1
	fi

}

echo "$(tput bold)--Compiling T4 templates--$(tput sgr 0)"

for var in "$@"
do
	if [ $var == "-a" ]; then
		compileT4 $core_configuration_template $core_configuration_output
		compileT4 $common_configuration_template $common_configuration_output
		compileT4 $script_configuration_template $script_configuration_output
		compileT4 $audio_configuration_template $audio_configuration_output
		compileT4 $video_configuration_template $video_configuration_output
		compileT4 $gpio_configuration_template $gpio_configuration_output
		compileT4 $push_configuration_template $push_configuration_output
	fi
done

# compile the project specific template file

compileT4 $template_file $implementation_path/$1/$1/$1ConfigurationTemplate.cs

if [ $? -eq 0 ];
then

touch "$cwd/$core_configuration_path/Shared.cs"
msbuild "$project_file"

fi

