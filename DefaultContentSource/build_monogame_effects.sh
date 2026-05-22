#!/bin/zsh

FX_DIR="./effects"
OUT_DIR="./DefaultContent/MG3.8.2Effects"
MGFXC="mgfxc"

if [ ! -d "$FX_DIR" ]; then
    echo "Error: Directory $FX_DIR not found."
    exit 1
fi

for file in "$FX_DIR"/*.fx; do
    if [ -f "$file" ]; then
        filename=$(basename "$file" .fx)
        
        $MGFXC "$FX_DIR/$filename.fx" "$OUT_DIR/$filename.mgfxo" /Profile:OpenGL
    fi
done

read -p "Press enter to continue"