paths=$(fd --exclude bin --exclude obj '.*\.cs$' ../)

for file in ${paths[@]}; do
    authors=$(git shortlog -n -s -- "${file}" | choose 1 | awk -v comment='\\/\\/' '{print comment,$0}' | sed -z 's/\n/\\n/g')
    sed -i '1s/^/\/\/ Authors:\n'"${authors}"'/' "${file}"
done
