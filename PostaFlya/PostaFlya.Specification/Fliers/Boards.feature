Feature: Boards
	In order for a range of people to see my flier
	As a participant
	I want to be able to add my flier to a range of boards


Scenario: Participant Creates Board
Given I am a BROWSER in PARTICIPANT ROLE
When I submit the following data for the BOARD:
	| BoardName | AcceptOthersFliers | RequireApprovalForFliers | TypeOfBoard  |                                      
	| MyBoard	| True               | True                     | InterestBoard |
Then a BOARD named MyBoard will be created
And the BOARD will allow Others to post FLIERS
And the BOARD will require approval for posted FLIERS
And the BOARD will have the status Approved

Scenario: Admin Approves Board
Given there is a public board named publicBoard that requires approval
And the BOARD has the status PendingApproval
And I am a PARTICIPANT with Admin ROLE
When I approve the BOARD
Then the BOARD will have the status Approved

Scenario: NonAdmin Cant Approve Board
Given I am a PARTICIPANT without Admin ROLE
Given I have created a public board named publicBoard that requires approval
And the BOARD has the status PendingApproval
When I approve the BOARD
Then the BOARD will have the status PendingApproval


Scenario: View Board
Given there is a public board named publicBoard that requires approval
When I navigate to the public view page for that Board
Then I will see the Information for that Board




