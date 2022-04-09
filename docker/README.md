# Set up ROS docker image

> **If you already have a working ROS environment, you don't need to follow the steps here. If not, it is recommended to use Docker to recreate the result in the paper.**

Refer to the following sections to connect to spin up a ROS environment in a docker container.
Since ROS decides during execution which ports to open (but we must explicitly tell Docker BEFORE starting the container which ports must be mapped), connecting to the ROS docker container can be quite challenging. Here are 2 methods shown.

## Using host network (Ubuntu only)

The following approach will remove the isolation of the docker container. This method should be generally avoided, but works with minimum effort. Refer [here](#using-a-virtual-private-network) for a more elegant solution.

1. Navigate to the root folder of this repo and run:

    ```bash
    docker build -t ros_melodic -f docker/ros_melodic/Dockerfile .
    docker run -it --rm --name ros_melodic --network host ros_melodic /bin/bash
    ```

    This will build the corresponding Docker image and start it.

2. Open a new terminal and log into the docker container with:

    ```bash
    docker exec -it ros_melodic /bin/bash
    ```

3. Start the server endpoint with the following command:

    ```bash
    roslaunch ros_tcp_endpoint endpoint.launch
    ```

    Once the server_endpoint has started, it will print something similar to `[INFO] [1603488341.950794]: Starting server on 0.0.0.0:10000` and `started core service [/rosout]`.

    > While using this launch file, your ROS_IP and ROS_TCP_PORT parameters are read from the file src/ros_tcp_endpoint/config/params.yaml. You can edit this file to adjust your settings - for example, this command will set the appropriate IP address for your machine:
    > `echo "ROS_IP: $(hostname -i)" > src/ros-tcp-endpoint/config/params.yaml`

    > Read more about rosparam YAML options [here](http://wiki.ros.org/rosparam).
    >
    > Read more about the ROS Parameter Server [here](http://wiki.ros.org/Parameter%20Server).

## Using VPN

This method will connect to the docker ROS container over a VPN.
It has been successfully tested on MacOS Monterey, but should also work on other OS.

Please be informed that an OpenVPN client will be necessary. Any client should be fine (on MacOS you can use [Tunnelblick](https://tunnelblick.net/downloads.html)).

If you already created the Docker container, please skip to Step 2.

1. Open a Terminal and create a docker network

    ```bash
    docker network create --subnet=172.25.0.0/16 docker-mac-network_default
    ```

    > You can check your available docker networks with the command `docker network ls`.
    > If you have the network created already, you do not need to do it again.

2. Go to the root folder of this repository and run

    ```bash
    docker-compose -f docker/docker-mac-network/docker-compose.yml -f docker/ros_melodic/docker-compose.yml up
    ```

    This will spin up a docker network:

    - a proxy container that forwards port 13194 to the OpenVPN container  
    - an OpenVPN container that has access to all containers within that docker network
    - a ROS container that directly runs `roscore`

    > Note: If you do this for the first time it might take a while to create the necessary keys.
    > A file `docker-mac-network/docker-for-mac.ovpn` will be created.

3. Connect to the VPN network by importing the file `docker-mac-network/docker-for-mac.ovpn` into your OpenVPN client.

    > **Important**: You might get an error about a bad compression type. If this is the case, comment out the line `comp-lzo no` within the file `docker-mac-network/config/openvpn.conf` and restart the docker container network in the previous step.

4. Open a new terminal and log into the ros docker container with:

    ```bash
    docker exec -it ros_melodic /bin/bash
    ```

    Then, start the server endpoint with the following command:

    ```bash
    roslaunch ros_tcp_endpoint endpoint.launch
    ```

> Note: If you have trouble in publishing/subscribing messages, it might be necessary to add the ROS docker container to your network configuration file. On Mac and Ubuntu, please add the following line to `\etc\hosts`:
>
>    ```bash
>    172.25.0.3    docker-ros
>    ```
