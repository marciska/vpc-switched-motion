# Getting Started

[![VMO](images/header.png)][YT]

## Structure

The repository is organized in folders with files as follows:

```sh
├── data/               # simulation results are saved in .mat files here
├── lib/                # all functions for this repository
├── simulink/           # simulink files
│   ├── matlabVPC.slx/  # matlab VPC Simulink file
│   └── unityVPC.slx/   # unity VPC simulink file
├── vpc_library/        # the VPC-core library
├── unityVPC/           # Unity project files
├── plot_results.m      # plots the figures in the paper
├── generateGPdata.m    # generates the GP datasets
├── startup.m           # loads all libraries and other files for simulation
└── init.m              # initializes all parameters
```

> In order to run any file, please make sure that `startup.m` has been called to load all libraries (either automatically during the start of MATLAB, or manually).

## How to generate the Gaussian Process Data

Run the file `generateGPdata.m`. It uses the technique from the paper

> M. Omainska, J. Yamauchi, T. Beckers, T. Hatanaka, S. Hirche, M. Fujita, “Gaussian process-based visual pursuit control with unknown target motion learning in three dimensions”. SICE Journal of Control, Measurement, and System Integration, Vol. 14, No. 1, pp. 116-127, 2020.

to generate the Rigid Body Motion data for the GP models used in this repository.
It will also optimize the hyperparameters and create a few animation windows to help design a trajectory.

## Run the simulation in the paper

> Before every simulation, please ensure that the parameters have been initialized by calling the script `init.m`.
> To plot the results in the paper, please run the script `plot_results.m`.

There are two ways to run the simulation.

### MATLAB simulation

This will run a simulation with switched trajectories within MATLAB only. Please note that factors like communication delays, air-resistance etc. are not considered here. To run this simulation, please start the Simulink file `matlabVPC.slx`.

### Unity simulation

This is a more practical simulation, as it will include factors that are likely to happen in the real world. Please ensure that you have already done the following:

- Created the ROS docker image as explained in [this guide](docker/README.md).
- Followed the unity setup guide [at the end of this file](#unity-setup-guide).
- Compiled the new ROS messages within the folder `ros/custom_msgs/` in MATLAB by the command:
    > `>> rosgenmsg('ros/custom_msgs/')`

Follow these steps in order to run the unity simulation:

1. (If no local ROS-environment is used) Setup the Docker-composed ROS network
    1. Start the ROS docker container as explained in [this guide](docker/README.md)

    2. Connect to the Docker-composed ROS network with [Tunnelblick](https://tunnelblick.net/downloads.html) or any other OpenVPN client.

    3. Open a new terminal and log into the ros docker container with:

        ```bash
        docker exec -it ros_melodic /bin/bash
        ```

        Then, start the server endpoint with the following command:

        ```bash
        roslaunch ros_tcp_endpoint endpoint.launch
        ```

2. Open the Unity Project and click on the triangle `Play` button. This will start the simulation, and we are ready to send the control inputs with MATLAB.

3. Open MATLAB, run `startup.m` and `init.m`, and connect to the ROS master within the Docker container:
    > `>> rosinit('docker-ros',11311,'NodeHost','192.168.255.6')`

4. Open the simulink file `unityVPC.slx` and run the simulation.

If you want to plot the results, please refer to `plot.results.m`.

## Unity Setup Guide

To setup Unity, please follow these steps:

1. Install Unity (`v2020.3+`).

2. Open the Unity Hub, select `Open -> Add project from disk` and navigate to the root folder of this repository to open the project folder `unityVPC`. Wait for Unity to open and install all dependencies.

3. Within Unity, open the `Package Manager` and ensure that `ROS-TCP-Connector` has been installed. If not, click the + button at the top left corner. Select "add package from git URL" and enter "https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector" to install the [ROS-TCP-Connector](https://github.com/Unity-Technologies/ROS-TCP-Connector) package. If you need a more in-depth guide, please follow the tutorial at the official [Unity Robotics Hub GitHub Project page](https://github.com/Unity-Technologies/Unity-Robotics-Hub).

4. Open `Robotics/ROS Settings` from the Unity menu bar, and set the `ROS IP Address` variable depending on your environment:

    - If you followed the guide for how to connect to a ROS docker container over a [virtual private network](docker/README.md#using-a-virtual-private-network), please change the `ROS IP Address` to `172.25.0.3`.
    - If you followed the guide for [how to remove the Docker isolation](docker/README.md#using-host-network-ubuntu-only), please leave the IP address as `127.0.0.1`.
    - If you have your own ROS environment, please change it to the PC that is running the ROS master.

[YT]:https://youtu.be/YxX8FoeyF8g
