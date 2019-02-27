# Projectors and Mirrors

A simple project showing how rays of light interact with mirrors, and how the physical and optical centers of the image are affected.

## Getting Started

Download the project and test it inside the Unity3D editor. It was developed using 2018.3.5f1.

## Explanation
The scene contains one physical projector and two mirrors. The target of the scene is to project over the box (sandbox).
The white ray is the projection of the physical forward of the projector, while the blue ray is the projection of the optical centre (different if there is a lens shift/tilt).

By rotating and moving the mirrors, the position and orientation of the virtual projector will adjust accordingly as long as the white ray hits the target surface. This will provide a solution as long as the white ray hits the target surface.

![No alt-text yet](Documentation/scene-description.png?raw=true "Scene Description")

All the work is done by the RayCastCamera script attached to the physical projector. It is possible to set some parameters, notably if it needs to be recomputed or display options.

![No alt-text yet](Documentation/display-options.png?raw=true "Solver and display options")

NOTE: Given that layers are problematic when doing SAR (because they are used for rendering), at the moment the ProjectionTarget and Mirror gameobjects must be tagged using the corresponding MonoBehaviour.