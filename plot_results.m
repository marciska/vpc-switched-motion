%% plot_results.m
% *Summary:* Plots the figures in the simulation section of the paper
%
% -----------
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
% January 2022
%
%------------- BEGIN CODE --------------

% load data
load('data/GP_1')
load('data/GP_2')
load('data/unitySimResult')

% simulation time
dt = out.dt;
tend = 20;
idx = 1:tend/dt;
t = out.tout(idx);

% RMSE
RMSE_VPC_GPfull = sqrt(mean(vecnorm(out.e1(idx,:),2,2)));
RMSE_VPC_switchedGP = sqrt(mean(vecnorm(out.e2(idx,:),2,2)));
disp(['RMSE (VPC+GPfull)    : ' num2str(RMSE_VPC_GPfull)])
disp(['RMSE (VPC+switchedGP): ' num2str(RMSE_VPC_switchedGP)])
disp(['Performance Increase : ' num2str((1-RMSE_VPC_switchedGP/RMSE_VPC_GPfull)*100) '%'])


%% Calculate upper-bound on Lipschitz constant
lw = 8; % linewidth

% trajectory params
eta1 = 0.5;
v1 = 1;
eta2 = 1.5;
v2 = 0.5;

% area to calculate in
step = 0.2;
x = -2:step:2;
y = -3.3:step:3.3;

% setup meshgrid
[X,Y] = meshgrid(x,y);
vel1 = zeros(size(X));
vel2 = zeros(size(X));

% evaluate function over grid
for i = 1:numel(X)
    vel1(i) = norm(vanderpol(v1, eta1, X(i), Y(i)));
    vel2(i) = norm(vanderpol(v2, eta2, X(i), Y(i)));
end

% calculate slopes
[ddx1,ddy1] = gradient(vel1,step);
[ddx2,ddy2] = gradient(vel2,step);
ddxy1 = sqrt(ddx1.^2 + ddy1.^2);
ddxy2 = sqrt(ddx2.^2 + ddy2.^2);

% plot
figure('Name','Trajectory & Dataset','NumberTitle','off',...
    'Units','normalized','Position',[.55 .2 .8 .8]);
tiledlayout(1,2);
ax2 = nexttile;
contourf(ax2,X,Y,ddxy1);
hold(ax2,'on')
plot3(ax2,Xv_(:,1) - 2,Xv_(:,2),Xv_(:,3),'Color','#e63946','LineWidth',lw);
xlim(ax2,[-2.1 2.1]);
ylim(ax2,[-3.3 3.3]);
ax2.FontSize = 25;
title(ax2,'$\left|\left|\frac{\partial \mathbf{V}^b_{wo,1}}{\partial \mathbf{x}}\right|\right|$','interpreter','latex','FontSize',25)
ax3 = nexttile;
contourf(ax3,X,Y,ddxy2);
hold(ax3,'on')
plot3(ax3,Xq_(:,1) - 2,Xq_(:,2),Xq_(:,3),'Color','#e63946','LineWidth',lw);
xlim(ax3,[-2.1 2.1]);
ylim(ax3,[-3.3 3.3]);
ax3.FontSize = 25;
title(ax3,'$\left|\left|\frac{\partial \mathbf{V}^b_{wo,2}}{\partial \mathbf{x}}\right|\right|$','interpreter','latex','FontSize',25)
cb = colorbar;
cb.Layout.Tile = 'east';
cb.FontSize = 20;



%% Trajectory
lw = 8; % linewidth
mw = 6; % marker linewidth
ms = 20; % marker size

fig = figure('Name','Trajectory & Dataset','NumberTitle','off',...
    'Units','normalized','Position',[.55 .2 .8 .5]);
ax1 = nexttile;
hold(ax1,'on')
traj_v = plot3(ax1,Xv_(:,1) - 2,Xv_(:,2),Xv_(:,3),'Color','#457b9d','LineWidth',lw);
traj_q = plot3(ax1,Xq_(:,1) - 2,Xq_(:,2),Xq_(:,3),'Color','#38b000','LineWidth',lw);
data_v = plot3(ax1,Xv(:,1) - 2,Xv(:,2),Xv(:,3),'x','Color','#e63946','MarkerSize',ms,'LineWidth',mw);
data_q = plot3(ax1,Xq(:,1) - 2,Xq(:,2),Xq(:,3),'x','Color','#fb8500','MarkerSize',ms,'LineWidth',mw);
ax1.FontSize = 30;
grid(ax1,'on')
xlim([-2.5, 2.5])
ylim([-3.5, 3.5])
xlabel(ax1,'$[${\boldmath${p}$}$_{wo}]_1$ [m]','interpreter', 'latex','FontSize',35)
ylabel(ax1,'$[${\boldmath${p}$}$_{wo}]_2$ [m]','interpreter', 'latex','FontSize',35)
zlabel(ax1,'$[${\boldmath${p}$}$_{wo}]_3$ [m]','interpreter', 'latex','FontSize',35)
legend(ax1,[traj_v, traj_q, data_v, data_q],...
    '$\eta_1 = 0.5, \ v_1 = 1$ \quad',...
    '$\eta_2 = 1.5, \ v_2 = 0.5$ \quad',...
    '$\mathcal{GP}_1$',...
    '$\mathcal{GP}_2$',...
    'Location', 'best',...
    'FontSize', 35,...
    'interpreter', 'latex','NumColumns',2,'Position',[0.32 0.48 0.34 0.15]);


print(fig, 'images/trajectory', '-depsc')


%% Error
lw = 8; % linewidth

fig = figure('Name','Error','NumberTitle','off',...
    'Units','normalized','Position',[0 .2 1 .7]);
tiledlayout(4,1);
ax = nexttile(1,[3 1]);
ax.FontSize = 35;
hold(ax,'on')
grid(ax,'on')
p1=plot(t,vecnorm(out.e1(idx,:),2,2),'LineWidth',lw,'color','#fb8500');
p2=plot(t,vecnorm(out.e2(idx,:),2,2),'LineWidth',lw,'color','#0077b6');
xlim(ax,[0 tend])
xticks(ax,[0 1.3 5.8 7.2 8 11 12.2 16.2 17.6 20])
ylabel(ax,'$\|${\boldmath${e}$}$\|$','interpreter', 'latex','FontSize',40)
lg=legend(ax,[p1 p2],sprintf('\\textit{Case 1} (MSE: %.3f)',RMSE_VPC_GPfull),sprintf('\\textit{Case 2} (MSE: %.3f)',RMSE_VPC_switchedGP),'interpreter', 'latex','FontSize',40);
HeightScaleFactor = 1.5;
NewHeight = lg.Position(4) * HeightScaleFactor;
lg.Position(2) = lg.Position(2) - (NewHeight - lg.Position(4));
lg.Position(4) = NewHeight;

ax = nexttile;
ax.FontSize = 35;
hold(ax,'on')
grid(ax,'on')
p2=plot(t,out.psi(idx,:),'--k','LineWidth',0.9*lw);
p1=plot(t,out.psi_est(idx,:),'LineWidth',lw,'color','#e63946');
xlabel(ax,'$t$ [s]', 'interpreter', 'latex','FontSize',40)
xlim(ax,[0 tend])
xticks(ax,[0 1.3 6.9 7.2 10.5 11.6 17.2 17.7 20])
xtickangle(ax,0)
xticklabels(ax,{'0','1.3','6.9    ','    7.2','10.5 ','11.6','17.2     ','     17.7','20'})
ylabel(ax,'$s(t)$','interpreter','latex','FontSize',40)
yticks(ax,[1 2])
lg = legend(ax,[p1 p2],'$\bar{\psi} \ \ $','$\psi$','interpreter', 'latex','FontSize',45,'NumColumns',2, 'Position',[0.6852 0.1497 0.1035 0.1026]);

print(fig, 'images/error', '-depsc')


%% Animation
figure('Name','Animation','NumberTitle','off',...
    'Units','normalized','Position',[.05 0.05 .8 .8]);
ax = gca;
hold(ax,'on')
traj_v = plot3(ax,Xv_(:,1),Xv_(:,2),Xv_(:,3),'Color','#457b9d','LineWidth',3);
traj_q = plot3(ax,Xq_(:,1),Xq_(:,2),Xq_(:,3),'Color','#38b000','LineWidth',3);
data_v = plot3(ax,Xv(:,1),Xv(:,2),Xv(:,3),'x','Color','#e63946','MarkerSize',10,'LineWidth',3);
data_q = plot3(ax,Xq(:,1),Xq(:,2),Xq(:,3),'x','Color','#fb8500','MarkerSize',10,'LineWidth',3);
animate(ax,out.tout,...
      {out.gwo(:,:,idx),agentdesign('target','show_trajectory','on','blocksize',0.7*ones(1,3))},...
      {out.gwc1(:,:,idx),agentdesign('agent','color',[1 0.65 0],'blocksize',0.7*ones(1,3)),out.gcobar1(:,:,idx),agentdesign('estimate','color',[1 0.65 0],'blocksize',0.7*ones(1,3))},...
      {out.gwc2(:,:,idx),agentdesign('agent','color',[1 0 0],'blocksize',0.7*ones(1,3)),out.gcobar2(:,:,idx),agentdesign('estimate','color',[1 0 0],'blocksize',0.7*ones(1,3))}...
      ...%'fps',60,'recorder',struct('file','videos/animation.mp4','profile','MPEG-4','quality',100)...
)
