# PersonalProjects
This Repository contains personal projects including Game Development and Circuit design and building.



Git Large File System Setup and Tracking: https://docs.github.com/en/repositories/working-with-files/managing-large-files/configuring-git-large-file-storage


STEPS

 git lfs install --skip-repo
 git clone https://github.com/khadflow/PersonalProjects.git


Notes:

# To fix slow or timed out git clone
git config --global http.postBuffer
git config --global http.postBuffer 524288000 # Example: 500MB buffer

# Broken Controller?
PlayerInputAssets component should be attached to the characters and the script should be reverted to one of the previous commits if an Upgrade to Unity has regenerated it incorrectly with CRLF (which happens often)

# Prevent LF and CRLF Conversions
git config --global core.autocrlf true # WINDOWS
git config --global core.autocrlf input # MAC