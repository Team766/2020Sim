InfiniteRechargeSim
===================

3D Sandbox Simulation for the 2020 <a href="http://www.firstinspires.org/robotics/frc">FIRST Robotics Competition</a> game, FIRST Infinite Recharge.

<a href="https://github.com/Team766/MaroonFramework/tree/master/src/main/java/com/team766/hal/simulator">The simulation HAL implementation</a> can be used to control the virtual robots from Java

## Building a release package

Increment the version number in Unity. Commit the changed version number in git
with a commit message that also includes the version number.

```
cd $SimRepoRoot
rm -rf build
mkdir build
cd build
mkdir sim
```

Build the simulator in Unity for WebGL and x86-64 server targets.

The webgl target should be saved into the `build/` directory with a name of
`webgl` and the server target should be saved into the `build/sim/` directory
with a name of `server`.

```
cp ../ReleasePackage/run.sh ../ReleasePackage/run_sim.py ../ReleasePackage/sim_server.py ./
mkdir html
cp ../ReleasePackage/index.html html/
mv webgl html/sim
COPYFILE_DISABLE=1 tar --exclude='._*' --no-xattrs -cz -f sim.tar.gz sim/ html/ run.sh run_sim.py sim_server.py
```

Make a new release on https://github.com/Team766/2020Sim/releases and attach
sim.tar.gz
