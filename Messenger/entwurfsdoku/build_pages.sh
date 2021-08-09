#!/usr/bin/bash

class_mappings=$(./get_classes.sh)

IFS=$'\n'

# TODO: Parallelize
for mapping in ${class_mappings}; do
    (
        file_name="pages/${mapping% *}.md"
        touch "${file_name}"

        echo '#Benutzte Pakete' >"${file_name}"
        # drop using and semicolon at end
        grep -E '^using .*;' "${mapping#* }" | choose 1 | choose -c :-2 >>"${file_name}"

        echo '#Importschnittstellen' >>"${file_name}"
        ../ASTProcessor/bin/Debug/net5.0/ASTProcessor "${mapping#* }" | sort -u >>"${file_name}"

        echo '#Exportschnittstellen' >>"${file_name}"
        ./get_methods.sh "${mapping#* }" >>"${file_name}"
    ) &
done

wait
