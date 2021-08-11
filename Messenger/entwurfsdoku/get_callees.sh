#!/usr/bin/bash

il_file=$(monodis ../Messenger.Core.Tests.MSTest/bin/Debug/net5.0/Messenger.Core.dll)
class_name="${1}"

if [[ -z "${class_name}" ]]; then
    echo "first argument must be class name to get callees from"
    exit 1
fi

get_class_body_from_il_file()
{
    local class_name="${1}"

    # start pattern matches:
    # .class public auto ansi beforefieldinit MicrosoftGraphService
    echo "${il_file}" | sed -n '/\.class.*'"${class_name}"'$/,/end of class .*'"${class_name}"'$/p'
    # end pattern matches:
    # } // end of class Messenger.Core.Services.MicrosoftGraphService
}

get_calls_from_il_class_body()
{
    echo "${1}" |
    # Retrieve lines with call or virtcall
    grep call |
    # Drop empty lines
    grep -v -e '^$' |
    # Drop leading IL identifiers
    choose 1: |
    # Drop leading 'call' and 'virtcall'
    sed -E -e 's/^call |callvirt //g' |
    # Drop leading 'instance'
    sed -E -e 's/^instance //g' |
    # Drop assembly names in brackets
    sed -e 's/\[[^][]*\]//g' |
    # Drop unnecessary tokens
    sed -E -e 's/class|object|valuetype//g' |
    # Drop leading whitespace
    sed -E -e 's/^\s*//g' |
    # Drop space after <
    sed -E -e 's/< /</g' |
    # Drop space after (
    sed -E -e 's/\( /\(/g' |
    # Drop weird tokens
    sed -E -e 's/`1//g' |
    sed -E -e 's/<!!0>//g' |
    sed -E -e 's/System.Func.*>/System.Func/g' |
    # Drop weird token(from async calls?)
    grep -v '!!' |
    # Deduplicate
    sort -u |
    cat
}

get_method_name_from_il_call()
{
    echo "${1}" |
        # Drop return type
        choose 1: |
        # Drop parent class
        choose -f :: 1: |
        # Drop parameters
        choose -f '\(' 0 |
        # Drop other weird calls...
        grep -v -E '^'\''.*'\''$' |
        # Drop template args
        choose -f '<' 0 |
        cat
}

get_path_of_class()
{
    grep "${1}.cs" class_mapping.txt | choose 1
}

get_explicitly_called_methods()
{
   IFS=$'\n'

    for il_call in ${1}; do
        method_name=$(get_method_name_from_il_call "${il_call}")
        if [[ -z "${method_name}" ]]; then
         continue
        fi

        if rg -NI --pcre2 "(?<=\W)${method_name}(?=\W)" "$(get_path_of_class ${class_name})" | grep -v -E '^\s*//' >/dev/null; then
            echo "${il_call}"
        fi
    done
    }

get_explicitly_called_methods "$(get_calls_from_il_class_body "$(get_class_body_from_il_file "${class_name}")")" | sort -u
