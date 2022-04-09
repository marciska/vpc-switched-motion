# Script has been edited from:
# https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/ros_unity_integration/

FROM ros:melodic-ros-base

ENV ROS_WORKSPACE=/catkin_ws

# Copy packages into workspace
COPY ros_packages/ros/ $ROS_WORKSPACE/src/
RUN git clone https://github.com/Unity-Technologies/ROS-TCP-Endpoint $ROS_WORKSPACE/src/ros_tcp_endpoint -b v0.6.0

# Install ping-tools
# check for incoming pings with: sudo tcpdump -i eth0 icmp and icmp[icmptype]=icmp-echo
RUN apt-get update && apt-get install iputils-ping net-tools nmap tcpdump nano -y

# Setup ROS workspace
COPY docker/ros_melodic/set-up-workspace /setup.sh
RUN chmod +x /setup.sh && /setup.sh && rm /setup.sh
WORKDIR $ROS_WORKSPACE

# Source the workspace on sign in
RUN echo ". devel/setup.bash" >> ~/.bashrc

# making sure the file modes are executable
RUN chmod +x src/ros_tcp_endpoint/src/ros_tcp_endpoint/*.py
