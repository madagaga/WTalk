# WTalk
Google Talk like client for Windows

## Summary
Client library (pcl) and desktop client for Google Hangouts written in c#
Based on Tom Dryer python library.

This project is new and needs many improvement (perf / features) 

ProtoJson parser is inspired by Marc Gravell work on ProtoBuf-net, and it can be improved.

This is more a proof of concept than a real app.

Final goal is to have a hangout client on windows phone and a very simple client on windows (like gtalk was).

Actually basic features are supported : 
- Login ... 
- Active convesation list
- Active conversation history
- Sending / receiving message

Next features :
- Notification (pop up or notification center)
- Read state (unread messages)
- Messages dates
- Image / link messages 

Other Features :
- new UI ? 
- Performance improvements 
- Code cleaning 
- full Api implementation ? 

Known issues :
- Sometimes messages are lost ... even if google response is OK.

## Initial project
Port of https://github.com/tdryer/hangups to c#.
