Nez Persistence
==========

Zero dependency persistence. Different persistence strategies are included so you can choose based on your specific needs. If you need human-readable, hand-editable persistence JSON is a good candidate. If you need ultra high performance persistence then the binary route is the way to go.

## JSON
JSON library that includes support for polymorphic data structures and reference tracking. For more information, see [the README](JSON_README.md).


## NSON
JSON-like format with most of the same features as the JSON library but is designed to be human readable and easy to hand-edit.


## High Performance Binary
Crazy efficient binary file formats. Requires a bit more work than JSON, but if performance is important (such as runtime state persistence) this is the way to go. For more information, see [the README](BINARY_README.md).