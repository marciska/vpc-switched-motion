%% init.m
% *Summary:* Defines parameters necessary for the unity simulation
%
% -----------
%
% Editor:
%   OMAINSKA Marco - Doctoral Student, Cybernetics
%       <marcoomainska@g.ecc.u-tokyo.ac.jp>
% Supervisor:
%   YAMAUCHI Junya - Assistant Professor
%       <junya_yamauchi@ipc.i.u-tokyo.ac.jp>
%
% Property of: Fujita-Yamauchi Lab, University of Tokyo, 2021
% e-mail: marcoomainska@g.ecc.u-tokyo.ac.jp
% Website: https://www.scl.ipc.i.u-tokyo.ac.jp
% January 2022
%
% ------------- BEGIN CODE -------------

% Note:
% It is assumed that you have already started the ROS Docker container, and
% that MATLAB can already send & receive ROS messages
% rosinit('docker-ros',11311,'NodeHost','192.168.255.6')

%% Parameters

% vpc gains
Ke = 17*eye(6); % estimation gain
Kc = 10*eye(6); % control gain

% desired pose
gd = [eye(4,3),[0 2 0 1]'];

% initial conditions
gco_init = mergepose(eye(3),[0 2 0]);

% focal length
lambda = 20;

% feature points
fp = [   0,  0,  0.5;
       0.5,  0,    0;
       0,    0, -0.5;
      -0.5,  0,    0];

% ROS message frequency [Hz]
ros_freq = 50;

% load GP models
load('data/GP_1')
load('data/GP_2')
load('data/GP_full')

% calculate maximum var norms
alpha = [0 1 0 0 0 0];
[~, cov1] = gp_calc([100 100 100 100 100 100], X1, Y1, @SEARD, sn1, hyp1);
[~, cov2] = gp_calc([100 100 100 100 100 100], X2, Y2, @SEARD, sn2, hyp2);
var_max = [norm(alpha*diag(cov1),2), norm(alpha*diag(cov2),2)];

% set switching constant
T = 0.05;

