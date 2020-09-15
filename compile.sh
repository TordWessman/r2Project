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
core_configuration_path="Src/Core/Settings"
T4_compiler_command="$T4_compiler_path -I $core_configuration_path"
core_configuration_template="$cwd/$core_configuration_path/CoreConfigurationTemplate.tt"
core_configuration_output="$cwd/$core_configuration_path/CoreConfigurationTemplate.cs"
audio_configuration_template="$cwd/Src/Audio/AudioConfigurationTemplate.tt"
audio_configuration_output="$cwd/Src/Audio/AudioConfigurationTemplate.cs"
video_configuration_template="$cwd/Src/Video/VideoConfigurationTemplate.tt"
video_configuration_output="$cwd/Src/Video/VideoConfigurationTemplate.cs"
gpio_configuration_template="$cwd/Src/GPIO/GPIOConfigurationTemplate.tt"
gpio_configuration_output="$cwd/Src/GPIO/GPIOConfigurationTemplate.cs"
common_configuration_template="$cwd/Src/Common/CommonConfigurationTemplate.tt"
common_configuration_output="$cwd/Src/Common/CommonConfigurationTemplate.cs"
script_configuration_template="$cwd/Src/Scripting/ScriptingConfigurationTemplate.tt"
script_configuration_output="$cwd/Src/Scripting/ScriptingConfigurationTemplate.cs"
push_configuration_template="$cwd/Src/PushNotifications/PushNotificationsConfigurationTemplate.tt"
push_configuration_output="$cwd/Src/PushNotifications/PushNotificationsConfigurationTemplate.cs"

template_file="$implementation_path/$1/$1/$1ConfigurationTemplate.tt"
project_file="$implementation_path/$1/$1.sln"

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

