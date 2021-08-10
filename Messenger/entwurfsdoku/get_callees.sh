#!/usr/bin/bash

calls=$(monodis ../Messenger.Core.Tests.MSTest/bin/Debug/net5.0/Messenger.Core.dll |
    grep call |
    grep -v -e '^$' |
    choose 1: |
    sed -E -e 's/^call |callvirt //g' |
    sed -E -e 's/^instance //g' |
    sed -e 's/\[[^][]*\]//g' |
    sed -E -e 's/class|object|valuetype//g' |
    sed -E -e 's/^\s*//g' |
    sed -E -e 's/< /</g' |
    sed -E -e 's/`1//g' |
    grep -v '( !!0&' |
    sort -u |
    cat)

get_method_name() {

    echo "${1}" |
        choose 1: |
        choose -f :: 1: |
        choose -f '\(' 0 |
        grep -v -E '^'\''.*'\''$' |
        choose -f '<' 0 |
        cat
}

IFS=$'\n'

method_names=$(
    (for call in ${calls}; do
        get_method_name "${call}"
    done) | sort -u
)

for method_name in ${method_names}; do
    if rg -NI --pcre2 -g '*.cs' "(?<=\W)${method_name}(?=\W)" ../ | grep -v -E '^\s*//' >/dev/null; then
        echo "${method_name}"
    fi
done
