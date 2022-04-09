function [Vbwo,dVbwo] = vanderpol(v,eta,x,y) %#codegen
%VANDERPOL_gradient calculates the gradient (transvel + angvel) at p
%
% Detailed Explanation:
%   none
%
% Remarks:
%   none
%
% -----------
%
% Inputs:
%   - v: test input [scalar]
%   - eta: data set [scalar]
%   - x: position [scalar]
%   - y: position [scalar]
%
% Outputs:
%   - Vbwo: body velocity [size 6]
%   - dVbwo: body acceleration [size 6]
%
% Example commands:
%   none
%
%
% Editor:
%   OMAINSKA Marco - Doctoral Student, Cybernetics
%       <marcoomainska@g.ecc.u-tokyo.ac.jp>
% Review:
%   YAMAUCHI Junya - Assistant Professor
%       <junya_yamauchi@ipc.i.u-tokyo.ac.jp>
%
% Property of: Fujita-Yamauchi Lab, University of Tokyo, 2022
% e-mail: marcoomainska@g.ecc.u-tokyo.ac.jp
% Website: https://www.scl.ipc.i.u-tokyo.ac.jp
% March 2022
%
%------------- BEGIN CODE --------------

% calculate derivatives
dx = v*y;
dy = -v*x + v*eta*(1-x^2)*y;
ddx = v*dy;
ddy = -v*dx + v*eta*(1-x^2)*dy - 2*v*eta*x*dx*y;
if dx^2+dy^2 < 10*eps
    wz = nan;
else
    wz = (dx*ddy-ddx*dy)/(dx^2+dy^2);
end

% calculate body velocity
Vbwo = [dx;
        dy;
        0;
        0;
        0;
        wz];

% calculate 1st derivative
if nargout == 2
    % calculate derivatives
    dddx = v*ddy;
    dddy = -v*ddx + v*eta*(1-x^2)*ddy - 2*v*eta*(y*dx^2 + x*ddx*y + 2*x*dx*dy);
    if dx^2+dy^2 < 10*eps
        dwz = nan;
    else
        dwz = ((dx*dddy-dddx*dy)*(dx^2+dy^2)-2*(dx*ddy-ddx*dy)*(dx*ddx-dy*ddy))/((dx^2+dy^2)^2);
    end
    
    % body acceleration
    dVbwo = [ddx;
             ddy;
             0;
             0;
             0;
             dwz];
end

%-------------- END CODE ---------------
end

