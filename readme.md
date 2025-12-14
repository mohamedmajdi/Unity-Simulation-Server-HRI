# Unity Simulation Server

A Unity-based simulation server for underwater robotics including a BlueROV with ROS2 integration.

## Features

### Simulation Environment
- **Realistic Underwater Physics**
  - Buoyancy simulation
  - Realistic motion dynamics
  - Collision detection
- **BlueROV2 Model**
  - Integrated camera
  - vehicle dynamics
- **Environment**
  - Cirtesu UJI pool (underwater environment)
  - Black box object

### ROS2 Integration
- **Velocity Subscriber** - `/bluerov1/cmd_vel` for robot control
- **Camera Publisher** - Real-time camera feed from BlueROV2
- **Odometry Publisher** - Position and orientation data
- **Collision Checker** - Real-time collision detection

## Prerequisites

### Required ROS2 Packages

1. **ROS TCP Endpoint** - Communication bridge between Unity and ROS2
   - Repository: [https://github.com/Unity-Technologies/ROS-TCP-Endpoint](https://github.com/Unity-Technologies/ROS-TCP-Endpoint)

2. **Teleop Twist Keyboard** - Keyboard teleoperation control
   - Repository: [https://github.com/ros2/teleop_twist_keyboard](https://github.com/ros2/teleop_twist_keyboard)

## Usage

### Step 1: Start ROS TCP Endpoint Server

Run the ROS TCP endpoint to establish communication with Unity:

```bash
ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=127.0.0.1 -p ROS_TCP_PORT:=10000
```

**Parameters:**
- `ROS_IP`: IP address for the ROS server (use `127.0.0.1` for local, or your machine's IP for network)
- `ROS_TCP_PORT`: TCP port for communication (default: `10000`). It should be same as inside Unity as well

### Step 2: Launch Unity Simulation

Open the Unity project and press Play to start the simulation server.

### Step 3: Control the BlueROV2

Use keyboard teleoperation to control the vehicle:

```bash
ros2 run teleop_twist_keyboard teleop_twist_keyboard --ros-args --remap cmd_vel:=/bluerov1/cmd_vel
```


## Dependencies
- Unity Robotics Hub packages
- ROS2 Humble
