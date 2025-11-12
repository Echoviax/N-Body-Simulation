This is a WIP project, and as such the readme will be rambling.  
  
I have a heavy interest in all things space related. For a project, my roommate made a compute shader in 2D with a bunch of 1 pixel boids. And seeing that, I wanted to make a 3D n-body simulation for simulation astronomical events.  
First issue I ran into was I have never used shaders outside of something like a healthbar. Second issue I run into is optimization. Gravitation is an O(n^2) function, which is really expensive.  
I am currently on the path of optimising this. The first step I took was adding a groupshared array, and changing my code to have as little square roots as possible. This allowed me to jump up from 20k particles comfortably to 100k particles. (This is on my old 3070)  
But I want millions of particles...  

In reading this paper (https://www.rath.us/m368k/hw5/gpu-gems-3--ch-31-N-body.pdf) and a few other sources, I find a lot of mentions of a Barnes-Hut simulation (https://en.wikipedia.org/wiki/Barnes%E2%80%93Hut_simulation) It turns out that in an optimal world, the Barnes-Hut simulation would run in O(n logn) time as opposed to O(n^2). This means that the processing time to run 100,000 particles (My current max with O(n^2)) would run upwards, and more than 50,000,000 particles (Proof? What's that). Of course this assumes that implementing an octree takes no overhead whatsoever, and also ignores the behemoth task of creating an octree on the GPU.  
We'll see where this goes.  

I'm also curious about some rendering things. I have a material that renders points and one that renders quads. I think the points would be more efficient (I'd like to say they obviously would, but I have no proof). But it seems the preview for both render always. So I'm curious if that is using extra resources. I like the quads as they allow me to zoom in, but I don't know how much processing power I am sacrificing by continuing to use quads.
