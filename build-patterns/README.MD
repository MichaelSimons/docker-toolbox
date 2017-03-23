## cp-output ##
### pros ###
- Captures build logic in a Dockerfile.  Important for non-trivial builds.
### cons ###
- Requires output on local directory
- Complicated to implement/understand
- Requires special handling to cleanup artifacts in build failure cases.


## nested-docker ##
### pros ###
- Does not require output on local directory
- Captures build logic in a Dockerfile.  Important for non-trivial builds.

### cons ###
- Complicated to grawk nested docker
- Need to be cautious with nested docker - some scenarios don't work well/aren't supported.


## src-volume ##
### pros ###
- Easiest to understand

### cons ###
- Requires output on local directory
