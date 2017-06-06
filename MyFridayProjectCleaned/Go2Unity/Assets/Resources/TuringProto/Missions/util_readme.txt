There are a few "util" scripts in this folder.

They have the file extension ".command",
which means they can either be executed from command-line,
or from and OSX finder window by just opening them like any other file. (eg double-click)

All of the .commands are simple wrappers around the python script "newSum.py".
To use newSum.py directly, run it from the command-line with the argument "--help".

Workflow for modifying challenge copy with these utils
------------------------------------------------------

This is critical. The tools are okay, but not flexible enough to allow multiple people working at once.

1. the copy editor tells everyone they're about to begin a round of copy edits.

2. the copy editor waits for everyone to say "cool, i'm all checked-in and won't work on any challenges until i hear from you".

3. the copy editor makes sure they're up-to-date with the latest from git.

4. the copy editor runs util_json_to_csv.command.
   this converts all the strings in all the user-facing missions into a single .CSV file.

   CRITICAL: no challenge changes may be made with the regular authoring tool between steps 4 and 6.

5. the copy editor uses google docs (or excel, but i don't recommend Numbers) to update the copy.
   
   CRITICAL: the rows in the spreadsheet must stay in the same order.
   ie, new rows may not be inserted, rows may not be deleted, rows may not be moved.
   this is a tool for editing the text of existing challenges/steps/hints only.
   to add or remove or re-arrange challenges/steps/hints, use the regular authoring tool,
   but not while also working in the CSV mode.

   When downloading the updated .CSV file (from google docs, eg),
   be sure to place the file in the same folder it came from,
   and you'll probably need to rename it to be the exact same file name as the original.

   At this point, you could do a git diff on the .CSV file, as a sanity-check.

   Note: the import tool identifies columns by their header,
   which means if you want you can add new columns, or even rearragne existing columns.
   Just keep the names of the current columns the same.

6. when done with copy edits, the copy editor runs util_csv_to_json.command.
   this will insert any string modifications from the .csv file back into the various .json files.

7. copy editor uses their favorite git diff tool to verify that changes to the mission files look sane.
   (you'll need to ignore whitespace for this to look meaningful)

8. copy editor commits & pushes/syncs their changes.

9. copy editor tells everyone they're done with the round of copy edits.

10. copy editor gains 5 xp for every edited string, and loses 30 xp for each git conflict.




util_json_to_csv.command
------------------------

this parses all the missions actively in the user-facing missions file,
and converts all the strings into a .csv file located in Go2/Go2Unity/Resources/Strings/challenges.csv.


util_csv_to_json.command
------------------------

this reverses the process util_json_to_csv.json, going from the .csv file back into the json.

util_json_to_po.command
-----------------------

this extracts all the strings from the user-facing missions and places them in Go2/Go2Unity/Resources/Strings/challenges.po.
note there are still some pending minor changes to the .po file format here.




