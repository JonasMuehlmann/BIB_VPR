paths=$(fd --exclude bin --exclude obj '.*\.cs$' ../)

for file in ${paths[@]}; do
    authors=$(git shortlog -n -s -- "${file}" | choose 1 | xargs -n1 ./real_name.sh | sort -u | sed -z 's/\n/, /g')
    if grep 'Authors:' "${file}" >/dev/null; then
        sed -i '0,/Authors:/ s/.*/\/\/ Authors: '"${authors}"'/' "${file}"
    else
        sed -i '1s/^/\/\/ Authors: '"${authors}"'\n/' "${file}"
    fi
done
