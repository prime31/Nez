#!/bin/bash


if [ "$#" -ne 1 ]
then
  echo "Usage: tag_and_release.sh NEW_VERSION_NUMBER"
  exit 1
fi


echo "new version: $1"

sed -E -i '' "s/id=\"Nez\" version=\"(.*)\"/id=\"Nez\" version=\"$1\"/g" 'Nez.FarseerPhysics/Nez.FarseerPhysics.nuspec'

git add Nez.FarseerPhysics/Nez.FarseerPhysics.nuspec
git commit -m 'updated Farseer nuspec with new tag reference'
git tag $1

