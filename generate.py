#!/usr/bin/python

import os
import argparse
import shutil

BOILERPLATE_PATH = "./Boilerplate"
EXTENSIONS = ['.csproj', '.cs', 'sln'] 
BOILERPLATE_NAME = "r2Boilerplate"
RESOURCES_NAME = "Resources"
PROJECT_DIR = os.path.join(BOILERPLATE_PATH, BOILERPLATE_NAME)
RESOURCES_DIR = os.path.join(BOILERPLATE_PATH, RESOURCES_NAME)

def replace_in_files(directory, in_str, replace_str, extensions):

	for root, _, files in os.walk(directory):
		for file in files:

			if any(file.endswith(ext) for ext in extensions):
				file_path = os.path.join(root, file)

			with open(file_path, 'r') as f:
				content = f.read()

			content = content.replace(replace_str, in_str)

			with open(file_path, 'w') as f:
				f.write(content)

			new_file_path = os.path.join(root, file.replace(replace_str, in_str))
			os.rename(file_path, new_file_path)

def copy_directory(src_dir, dest_dir):
	shutil.copytree(src_dir, dest_dir)

if __name__ == "__main__":

	parser = argparse.ArgumentParser(description='Generate a boilerplate implementation in parent directory')
	parser.add_argument('project_name', help='Name of the project (Only valid characters, exluding whitespaces etc)')
	args = parser.parse_args()

	project_name = args.project_name
	parent_directory = ".."
	project_target_directory = os.path.join(parent_directory, project_name)
	resources_target_directory = os.path.join(parent_directory, RESOURCES_NAME)

	copy_directory(PROJECT_DIR, project_target_directory)
	copy_directory(RESOURCES_DIR, resources_target_directory)

	print("Generating project in {project_target_directory}")

	replace_in_files(project_target_directory, BOILERPLATE_NAME, project_name, EXTENSIONS)