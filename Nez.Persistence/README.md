Nez Persistence
==========

Zero dependency persistence. Different persistence strategies are included so you can choose based on your specific needs. If you need human-readable, hand-editable persistence JSON is a good candidate. If you need ultra high performance persistence then the binary route is the way to go.

## JSON
Kickass JSON library that includes support for polymorphic data structures and reference tracking. For more information, see [the README](JSON_README.md).


## High Performance Binary
Crazy efficient binary file formats. Requires a bit more work than JSON, but if performance is important (such as runtime state persistence) this is the way to go. For more information, see [the README](BINARY_README.md).