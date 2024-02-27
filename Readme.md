### How to start it locally
1. Download .net 8 version
2. Open solution and restore all Nuget packages
3. Run solution with Debug optimizations or with Release optimizations

### How to run it with Docker
1. Download and install Docker 
2. Open docker file's root folder in cmd\pwsh\bash\zsh
3. Fire command `docker run .`


### What can be improve?
The test task was completed swiftly, with the highest possible quality. 
Ideally, it would have been necessary to conduct a performance measurement and replace some sections of the code. 
For example, the line "body = await idsResponse.Content.ReadFromJsonAsync<List<int>>();" could have been rewritten 
with parsing string to array instead of deserialization as json, which might have yielded better results.

The use of "Parallel.ForEach" does not always benefit, and this loop could have been replaced with a synchronous loop,
which might also have been more efficient. Similarly, the use of a ConcurrentBag also leads to additional performance 
losses; here, it would have been worth considering another implementation to prevent race conditions.