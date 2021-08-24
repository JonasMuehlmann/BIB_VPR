#!/usr/bin/bash
ctags -x --languages=c\# --exclude='*/obj/*' --exclude='*/bin/*' --c\#-kinds=c -R ../ | choose 0 3 > class_mapping.txt
