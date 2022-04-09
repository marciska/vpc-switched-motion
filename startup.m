%% startup.m
% *Summary:* Loads all necessary libraries and other files for simulation
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

% init core VPC-Library
run('vpc_library/startup.m')

% add project root folder
libDir = fileparts(mfilename('fullpath'));
addpath(libDir)

% create output directories if not yet existent
dirs = ["data", "images", "videos"];
for d = dirs
    [~, ~, ~] = mkdir(fullfile(libDir,d));
end

% add paths to sub-dependencies
fprintf('Loading other libraries...\n')
dirs = ["data", "lib", "simulink", "ros"];
for d = dirs
    addpath(genpath(fullfile(libDir,d)))
end

% clear variables
clear d dirs libDir

fprintf('[done]\n\n')

% -------------- END CODE --------------
