FNA Compatibility FNA兼容
==========
为了让FNA项目准备起来更快, 我们分离开了 FNA 特定的项目 *.FNA.csproj. 注意你仍然需要安装[这个文档](https://github.com/FNA-XNA/FNA/wiki/1:-Download-and-Update-FNA)里的每一个 FNA 本机库. [MonoGameCompat class](../../Nez.Portable/Utils/MonoGameCompat.cs)也被包含了进来, 其中包含拓展方法, 是一些 MonoGame 里很常用的但是 FNA 里没有的方法.

见[README.cn.md](../../README.cn.md#using-nez-with-fna)中 "使用 FNA 的 Nez" 部分