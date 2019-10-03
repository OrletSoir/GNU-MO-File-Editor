# GNU-MO-File-Editor
An editor for GNU .mo files

----

While working on the next addition to the ever-growing Mermaid's Wrath and Mermaid's Fashion mods I have created an automated tool to modify the global.mo file that contains all the text strings found in the game client, including, but not limited to, ship names, captain names, and so on. Biggest part of the work included creating a module capable of reading, modifying, and saving the .mo file format without involving any third-party or external conversion tools.

Luckily, .mo file actually uses an [open-source format under GNU license](https://www.gnu.org/software/gettext/manual/html_node/MO-Files.html#MO-Files), so it wasn't too hard to create a bit of code that handles the format natively and in-memory. From there it was just an easy extra step of adding the extra code for automated replacement of strings. However, during the work, I figured it would be nice to provide an all-in-one tool for editing the .mo files for the WoWS modder community, and such the GNU MO File Editor tool has been born, which I hereby present to you!

The usage should be pretty straight-forward: launch the program, load your .mo file you want to edit, and you shall be presented with a grid of text, as illustrated in a *screenshot below*:

*(one of these days i'll figure how to add images to here; for now you can find it [here](https://forum.worldofwarships.com/topic/154616-mo-file-editor-v12/?tab=comments#comment-4714050))*

Only the "Value" column data can be edited, because the client relies on the ID strings to find the necessary data. The text box in the top-right corner allows for text search in the data (both ID and Value fields are searched) in forward and reverse manner. Result row will be highlighted fully.

Once done editing, save your file via the File menu.
