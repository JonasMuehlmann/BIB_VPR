#!/usr/bin/bash

class_mappings=$(./get_classes.sh)

IFS=$'\n'

for mapping in ${class_mappings}; do
    file_name="pages/${mapping% *}.md"
    touch "${file_name}"

    echo '#Benutzte Pakete' >"${file_name}"
    # drop using and semicolon at end
    grep -E '^using .*;' "${mapping#* }" | choose 1 | choose -c :-2 >>"${file_name}"

    echo '#Exportschnittstellen' >>"${file_name}"
    ./get_methods.sh "${mapping#* }" >>"${file_name}"
done
