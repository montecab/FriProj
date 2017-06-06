four files in this folder are symbolic links:
  * piShared.meta -> ../../../piUnityShared/piShared.meta
  * piShared      -> ../../../piUnityShared/piShared
  * Plugins.meta  -> ../../../piUnityShared/Plugins.meta
  * Plugins       -> ../../../piUnityShared/Plugins

the folder 'piUnityShared' corresponds to the git reposity https://github.com/playi/piUnityShared.
it may be that your folder structure is different than this, and you'll need to adjust the symlinks.
if this is the case, you should probably read-up on the shell command "ln -s".

- orion

