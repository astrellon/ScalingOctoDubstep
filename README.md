ScalingOctoDubstep
==================

ScalingOctoDubstep (SOD) is currently planned to a be a full space game. However it's current state is more of a Linux like interface. This Linux like interface is aimed to be a core part of the game, where a ship is fully programmable using a relatively simple API ingame.

The idea is that each in game ship has its own computer (or multiple computers even) that are of course independant of each other. They can communicate with connected devices such as sensors, engines, weapons, etc. While this is planned to be setup similar to what a real Linux/hardware layer would be like, it is also intended to be simple and extendable such that it can work within a game environment. The usual trade off being that it's more restrictive since it is expected (and designed so) that all devices work via a common interface that allows them to describe themselves and be controlled as such.

Main inspiration is all those Sci-Fi TV shows and movies that show the ships systems and peripheral being programmed or re-programmed within a matter of minutes to do something else. Want a probe that follows some specific course, easy, change that probes basic programming. Want the shields to be on a rotating frequency, easy change a few lines in the shields service. I realise this is rather ambitious but I feel like there is potential if the right balance between flexibility and simplicity is found for making programmable ingame systems.

This is currently in a very, very early stage but it shouldn't take too long to bring it to the point of being useful to some degree.

Current features (may not fully work):
- Simple Bash like interface.
- Small subset of VT100 emulation.
- Small subset of coreutils.
- Example of scripting integration via Lua.
- Isolation of file system from the users main file system.
- Symlinks.
- Fork of NLua and KopiLua with features that allow for multiple independant Lua instances that cannot talk to each other and live in their own world (rather than sharing statics).

Next step features:
- More robust of all of the above.
- Example of reading data and sending commands to an external 'device'.
- Custom file system that would sit in memory and would not use the hosts file system at all.
- Better pathing support, currently rather hacky but should at least prevent the in game computer from ever getting outside of its root folder.
- Better Bash support (currently only doing 1 level of > or |):
-- Better autocomplete

Screenshots
===========

![Example of calling Lua script](http://i.imgur.com/5Wkh4O9.png "Example showing calling Lua scripts, use of #! (shebang) and executing from $PATH.")

![Example showing basic file system](http://i.imgur.com/CJjoSmd.png "Example showing symlinks, changing folders and updating files.")
