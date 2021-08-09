#!/usr/bin/bash

class_mappings=$(./get_classes.sh)

# Split array on \n instead of space
IFS=$'\n'

for mapping in ${class_mappings}; do
    # Start command in subshell and put it in background
    (
        file_name="pages/${mapping% *}.md"
        touch "${file_name}"

        echo '#Benutzte Pakete' >"${file_name}"
        # drop 'using' and semicolon at end
        grep -E '^using .*;' "${mapping#* }" | choose 1 | choose -c :-2 >>"${file_name}"

        echo '#Importschnittstellen' >>"${file_name}"
        ../ASTProcessor/bin/Debug/net5.0/ASTProcessor "${mapping#* }" | sort -u >>"${file_name}"

        echo '#Exportschnittstellen' >>"${file_name}"
        ./get_methods.sh "${mapping#* }" >>"${file_name}"
    ) &
done

# Wait for all background tasks
wait
