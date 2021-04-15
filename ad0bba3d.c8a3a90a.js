(window.webpackJsonp=window.webpackJsonp||[]).push([[22],{108:function(e,t,n){"use strict";n.d(t,"a",(function(){return p})),n.d(t,"b",(function(){return m}));var r=n(0),i=n.n(r);function a(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function o(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function s(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?o(Object(n),!0).forEach((function(t){a(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):o(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function l(e,t){if(null==e)return{};var n,r,i=function(e,t){if(null==e)return{};var n,r,i={},a=Object.keys(e);for(r=0;r<a.length;r++)n=a[r],t.indexOf(n)>=0||(i[n]=e[n]);return i}(e,t);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);for(r=0;r<a.length;r++)n=a[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(i[n]=e[n])}return i}var c=i.a.createContext({}),u=function(e){var t=i.a.useContext(c),n=t;return e&&(n="function"==typeof e?e(t):s(s({},t),e)),n},p=function(e){var t=u(e.components);return i.a.createElement(c.Provider,{value:t},e.children)},d={inlineCode:"code",wrapper:function(e){var t=e.children;return i.a.createElement(i.a.Fragment,{},t)}},b=i.a.forwardRef((function(e,t){var n=e.components,r=e.mdxType,a=e.originalType,o=e.parentName,c=l(e,["components","mdxType","originalType","parentName"]),p=u(n),b=r,m=p["".concat(o,".").concat(b)]||p[b]||d[b]||a;return n?i.a.createElement(m,s(s({ref:t},c),{},{components:n})):i.a.createElement(m,s({ref:t},c))}));function m(e,t){var n=arguments,r=t&&t.mdxType;if("string"==typeof e||r){var a=n.length,o=new Array(a);o[0]=b;var s={};for(var l in t)hasOwnProperty.call(t,l)&&(s[l]=t[l]);s.originalType=e,s.mdxType="string"==typeof e?e:r,o[1]=s;for(var c=2;c<a;c++)o[c]=n[c];return i.a.createElement.apply(null,o)}return i.a.createElement.apply(null,n)}b.displayName="MDXCreateElement"},90:function(e,t,n){"use strict";n.r(t),n.d(t,"frontMatter",(function(){return a})),n.d(t,"metadata",(function(){return o})),n.d(t,"toc",(function(){return s})),n.d(t,"default",(function(){return c}));var r=n(3),i=(n(0),n(108));const a={id:"introduction",title:"Introduction",sidebar_label:"Introduction",slug:"/"},o={unversionedId:"about/introduction",id:"about/introduction",isDocsHomePage:!1,title:"Introduction",description:"Nez aims to be a feature-rich 2D framework that sits on top of MonoGame/FNA. It provides a solid base for you to build a 2D game on. Some of the many features it includes are:",source:"@site/docs\\about\\introduction.md",slug:"/",permalink:"/Nez/docs/",editUrl:"https://github.com/prime31/Nez/edit/master/Nez.github.io/docs/about/introduction.md",version:"current",sidebar_label:"Introduction",sidebar:"someSidebar",next:{title:"Samples",permalink:"/Nez/docs/about/Samples"}},s=[],l={toc:s};function c({components:e,...t}){return Object(i.b)("wrapper",Object(r.a)({},l,t,{components:e,mdxType:"MDXLayout"}),Object(i.b)("p",null,"Nez aims to be a feature-rich 2D framework that sits on top of MonoGame/FNA. It provides a solid base for you to build a 2D game on. Some of the many features it includes are:"),Object(i.b)("ul",null,Object(i.b)("li",{parentName:"ul"},"Scene/Entity/Component system with Component render layer tracking"),Object(i.b)("li",{parentName:"ul"},"SpatialHash for super fast broadphase physics lookups. You won't ever see it since it works behind the scenes but you'll love it nonetheless since it makes finding everything in your proximity crazy fast via raycasts or overlap checks."),Object(i.b)("li",{parentName:"ul"},"AABB, circle and polygon collision/trigger detection"),Object(i.b)("li",{parentName:"ul"},"Farseer Physics (based on Box2D) integration for when you need a full physics simulation"),Object(i.b)("li",{parentName:"ul"},"efficient coroutines for breaking up large tasks across multiple frames or animation timing (Core.startCoroutine)"),Object(i.b)("li",{parentName:"ul"},"in-game debug console extendable by adding an attribute to any static method. Just press the tilde key like in the old days with Quake. Out of the box, it includes a visual physics debugging system, asset tracker, basic profiler and more. Just type 'help' to see all the commands or type 'help COMMAND' to see specific hints."),Object(i.b)("li",{parentName:"ul"},"Dear ImGui in-game debug panels with the ability to wire up your own ImGui windows via attributes"),Object(i.b)("li",{parentName:"ul"},"in-game Component inspector. Open the debug console and use the command ",Object(i.b)("inlineCode",{parentName:"li"},"inspect ENTITY_NAME")," to display and edit fields/properties and call methods with a button click."),Object(i.b)("li",{parentName:"ul"},"Nez.Persistence JSON, NSON (strongly typed, human readable JSON-like syntax) and binary serialization. JSON/NSON includes the ability to automatically resolve references and deal with polymorphic classes"),Object(i.b)("li",{parentName:"ul"},"extensible rendering system. Add/remove Renderers and PostProcessors as needed. Renderables are sorted by render layer first then layer depth for maximum flexibility out of the box with the ability to add your own custom sorter."),Object(i.b)("li",{parentName:"ul"},"pathfinding support via Astar and Breadth First Search for tilemaps or your own custom format"),Object(i.b)("li",{parentName:"ul"},"deferred lighting engine with normal map support and both runtime and offline normal map generation"),Object(i.b)("li",{parentName:"ul"},"tween system. Tween any int/float/Vector/quaternion/color/rectangle field or property."),Object(i.b)("li",{parentName:"ul"},"sprites with sprite animations, scrolling sprites, repeating sprites and sprite trails"),Object(i.b)("li",{parentName:"ul"},"flexible line renderer with configurable end caps including super smooth rounded edges or lightning bolt-like sharp edges"),Object(i.b)("li",{parentName:"ul"},"powerful particle system with added support for importing Particle Designer files at runtime"),Object(i.b)("li",{parentName:"ul"},"optimized event emitter (",Object(i.b)("inlineCode",{parentName:"li"},"Emitter")," class) for core events that you can also add to any class of your own"),Object(i.b)("li",{parentName:"ul"},"scheduler for delayed and repeating tasks (",Object(i.b)("inlineCode",{parentName:"li"},"Core.schedule")," method)"),Object(i.b)("li",{parentName:"ul"},"per-Scene content managers. Load your scene-specific content then forget about it. Nez will unload it for you when you change scenes."),Object(i.b)("li",{parentName:"ul"},"customizable Scene transition system with several built in transitions"),Object(i.b)("li",{parentName:"ul"},"Verlet physics bodies for super fun, constraint-to-particle squishy physics"),Object(i.b)("li",{parentName:"ul"},"tons more stuff")))}c.isMDXComponent=!0}}]);