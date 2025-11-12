This is a WIP project, and as such the readme will be rambling.  
  
I have a heavy interest in all things space related. For a project, my roommate made a compute shader in 2D with a bunch of 1 pixel boids. And seeing that, I wanted to make a 3D n-body simulation for simulation astronomical events.  
First issue I ran into was I have never used shaders outside of something like a healthbar. Second issue I run into is optimization. Gravitation is an O(n^2) function, which is really expensive.  
I am currently on the path of optimising this. The first step I took was adding a groupshared array, and changing my code to have as little square roots as possible. This allowed me to jump up from 20k particles comfortably to 100k particles. (This is on my old 3070)  
But I want millions of particles...
