# Orchard Abstractions Readme



## Project Description

An Orchard module for providing higher-level abstractions to Orchard services.


## Documentation

Orchard Abstractions aims to simplify the usage of Orchard's APIs for the most common scenarios by providing abstractions above the built-in services.

For detailed, documented examples on how to use this module's feature's see the separate [examples module](https://github.com/Lombiq/Orchard-Abstractions-Examples).


## Quick Parts

Quick Parts let you develop content parts with ease: to write a content part you'll only need to implement the part class and the necessary templates for displaying or editing its content. No need to write or even understand or know about drivers, shapes, handlers, records, migrations, infoset storage, placement and the finer details of part development.

A quick part can be gradually customized and extended with standard content part APIs. If you later decide to change your implementation to the standard content part way it can be made with minor modifications and you won't loose your data.

Quick Parts get the following automatically:

- Storage for every public virtual property (in InfosetPart)
- Generated display and editor shapes, where the shapes are conventionally named - (Parts_PartName for display and Parts_PartName_Edit for editors). You only have to write the templates themselves.
- Default placement for display and editor shapes
- Part is made attachable
- Ability to add logic to run when the part is displayed


## Quick Widgets

Building on Quick Parts this feature enabled very quick widget development. For every quick widget part a corresponding widget content type is created automatically.

The module's source is available in two public source repositories, automatically mirrored in both directions with [Git-hg Mirror](https://githgmirror.com):

- [https://bitbucket.org/Lombiq/orchard-abstractions](https://bitbucket.org/Lombiq/orchard-abstractions) (Mercurial repository)
- [https://github.com/Lombiq/Orchard-Abstractions](https://github.com/Lombiq/Orchard-Abstractions) (Git repository)

Bug reports, feature requests and comments are warmly welcome, **please do so via GitHub**.
Feel free to send pull requests too, no matter which source repository you choose for this purpose.

This project is developed by [Lombiq Technologies Ltd](http://lombiq.com/). Commercial-grade support is available through Lombiq.