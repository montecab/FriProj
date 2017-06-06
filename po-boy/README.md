# Po-Boy 

![Po' boy](po-boy.jpg)

This is a repo of static PO files to be used in our mobile apps. The purpose of this repository is so we can live-update translations without having to release new versions of apps.

## File structure

The structure of PO files within this repo should follow a format of `/:appName/:version/<filename>`. This will correspond 1:1 to HTTP requests made to the PO file server.

Our PO file server hooks into this repo, so any push made here automatically gets picked up and published.

## Branching

The following branches are consumed by these environments:

| branch | env | endpoint | smartling project |
| --- | --- | --- | --- |
| master | alpha | https://alpha-popo.makewonder.com | Wonder Workshop - Mobile |
| prod | prod | https://popo.makewonder.com | Wonder Workshop - Mobile |
| dev | local | (no canonical url) | Wonder Workshop - Sandbox |

## Tagging

Any time a push is made, the PO does a diff between the latest commit and the latest tag on the branch of its environment. 

The source of truth use by the PO file server is the *latest* tag on this repository that exists within its respective branch.

If there has been a key (a `msgid`) that has been modified, the server will not serve PO files from this commit. We will also publish a message to Slack notifying people about it. If the `msgid` change was intentional, this would be fixed by publishing a new tag on the branch.

