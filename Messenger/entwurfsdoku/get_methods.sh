#!/usr/bin/bash

path=$1

ctags -x --c\#-kinds=m "${path}" | choose 4: | grep public
