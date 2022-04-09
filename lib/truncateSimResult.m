function trunc_simdata = truncateSimResult(simdata,tend)
%TRUNCATESIMRESULT truncates the simulink output by the given time
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
%   - simdata: Simulink data object [SimulationOutput]
%   - tend: cut-off time [scalar]
%
% Outputs:
%   - trunc_simdata: struct of truncated data fields [struct]
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

% Get time indices
trunc_simdata.dt = simdata.SimulationMetadata.ModelInfo.SolverInfo.FixedStepSize;
idx = 1:tend/trunc_simdata.dt;
trunc_simdata.tout = simdata.tout(idx);

% convert all other elements
fn = simdata.who;
ignoredFields = ["tout"]; %#ok<NBRAK>
for i = 1:numel(fn)
    if ~any(strcmp(ignoredFields,fn{i}))
        field = simdata.(fn{i}).signals.values;
        switch ndims(field)
            case 1
                trunc_simdata.(fn{i}) = field(idx);
            case 2
                trunc_simdata.(fn{i}) = field(idx,:);
            case 3
                trunc_simdata.(fn{i}) = field(:,:,idx);
            otherwise
                trunc_simdata.(fn{i}) = eval(['field(' [repmat(':,',[1 ndims(field)-1]) num2str(idx(1)) ':' num2str(idx(end))] ')']);
        end
    end
end

end

