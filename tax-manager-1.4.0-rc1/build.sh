#!/usr/bin/env bash

####### SCRIPT CONFIGURATION ######

set -e

project_name="TaxManager"
azure_devops_resourceId="499b84ac-1321-427f-aa17-267ca6975798"
dockerfile_path="./Dockerfile"  # Path to your Dockerfile
image_name="my-docker-image"    # Name for your Docker image
package_version="1.0.0"  # Set the package version or retrieve it dynamically

######### HELPERS ########

# Check the operating system and set temp_folder
function __osinfo() {
    case "$OSTYPE" in
        darwin*) OS="OSX";;
        linux*) OS="LINUX" ;;
        msys*|cygwin*) OS="WINDOWS"; temp_folder="${TEMP}" ;;
        *) echo "Unknown OS: $OSTYPE"; exit 1 ;;
    esac
}

# Check if docker has been installed.
function __check_docker() {
    if ! command -v docker &> /dev/null
    then
        echo "Docker not be found."
        echo "Please install Docker from https://docs.docker.com/get-docker/"
        exit
    fi
}

# Check if a binary exists and install JQ if needed
function __initializeJQ() {
    if ! command -v jq &> /dev/null; then
        if [[ "$OS" == "WINDOWS" ]]; then
            curl -L -o "$temp_folder/jq.exe" https://github.com/stedolan/jq/releases/latest/download/jq-win64.exe
        elif [[ "$OS" == "OSX" ]]; then
            curl -L -o /usr/local/bin/jq https://github.com/stedolan/jq/releases/latest/download/jq-osx-amd64
            sudo chmod +x /usr/local/bin/jq
        else
            sudo apt-get install -y jq
        fi
    fi
}

# Get the FEED_ACCESSTOKEN using Azure CLI
function __get_feed_accesstoken() {
    local token
    __initializeJQ
    az login -o none
    azure_devops_pat=$(az account get-access-token --resource $azure_devops_resourceId --output json)
    if [[ "$OS" == "WINDOWS" ]]; then
        token=$(echo $azure_devops_pat | $temp_folder/jq.exe --raw-output .accessToken)
    else
        token=$(echo $azure_devops_pat | jq --raw-output .accessToken)
    fi
    echo $token
}

 # Check if value is null or empty
 function __checkValueIsNullOrEmpty() {
    local var_value="$1"
    if [[ "$var_value" == "null" || -z "$var_value" ]]; then
        return 0
    else
        return 1
    fi
 }

##################################

Help() {
    echo
    echo ""
    echo "$project_name"
    echo " Syntax: [-h]"
    echo " Options:"
    echo " h     Print this Help."
    echo " d     Perform a Dry Run without actually executing commands."
    echo
    echo " Commands:"
    echo "./build.sh validate                validate the docker image used for deployment"
    echo
}

######### LOCAL RUN ########

function validate() {
    if  __checkValueIsNullOrEmpty "$azure_devops_resourceId"; then
        echo -e "\033[31m Error: Invalid configuration for 'azureDevopsResourceId'. Please check 'config.json' \033[0m"
        exit 1
    fi

    # Ensure Dockerfile exists
    if [[ ! -f "$dockerfile_path" ]]; then
        echo -e "\033[31m Error: Dockerfile not found at path '$dockerfile_path'. \033[0m"
        exit 1
    fi

    export FEED_ACCESSTOKEN=$(__get_feed_accesstoken "$azure_devops_resourceId")
    echo "Warning: this function is used to validate the container image."

     # Build the Docker image
    if docker build --pull --no-cache \
            --build-arg PACKAGE_VERSION="$package_version" \
            --build-arg FEED_ACCESSTOKEN="$FEED_ACCESSTOKEN" \
            -t "$image_name" -f "$dockerfile_path" .; then

        echo -e "\033[32m Docker image validated successfully. \033[0m"
        echo "Cleaning up Docker image..."
        docker rmi "$image_name"
        echo -e "\033[32m Docker image removed to save space. \033[0m"
    else
        echo -e "\033[31m Docker build failed. \033[0m"
        exit 1
    fi
}


###### MAIN PROGRAM ######
while getopts ":h" option; do
   case $option in
      h) # display Help
         Help
         exit;;
     \?) # incorrect option
         echo "Error: Invalid option"
         exit;;
   esac
done

function main() {
     __check_docker
     __osinfo
}
main

# Please leave this at the bottom of this script 
# It allows functions in this script to be called from the command line.
# Check if the function declared in command-line exists
if declare -f "$1" >/dev/null; then
    # call functions from command-line
    "$@"
else
    # Show a helpful error
    echo "Command '$1' not recognized" >&2
    Help
    exit 1
fi
