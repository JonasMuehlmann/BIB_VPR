
paths=$(fd --exclude bin --exclude obj '.*\.cs$' ../)

(for file in ${paths[@]}; do
   git shortlog -n -s -- "${file}" | choose 1
done) | sort -u
