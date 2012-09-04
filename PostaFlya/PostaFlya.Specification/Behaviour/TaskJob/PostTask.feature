Feature: Post a TASK
	As a PARTICIPANT with IdentityVerified role 
	I want to be able to POST a TASK FLYA 
	so that other PARTICIPANTS with TASKFLYA role can BID for that TASK

Scenario: Create Flier With TaskJob 
Given i have navigated to the CREATE PAGE for a FLIER TYPE TaskJob
When I SUBMIT the data for that FLIER 
Then the new FLIER will be created for behviour TaskJob
And the FLIER STATUS will be PENDING

Scenario: Create TaskJob Behaviour attached to Flier
Given I am a PARTICIPANT with IdentityVerified ROLE
And I have created a FLIER of BEHAVIOUR TaskJob 
When I submit the following data for the TASKJOB:
  | CostOverhead | MaxAmount | ExtraLocations |
  | 0            | 100       |                |
Then the TASKJOB will be stored for the FLIER


