Feature: Boards
	In order for a range of people to see my flier
	As a participant
	I want to be able to add my flier to a range of boards


Scenario: Participant Creates Board
Given i am an existing BROWSER with PARTICIPANT ROLE
When I submit the following data for the BOARD:
	| BoardName | AcceptOthersFliers | RequireApprovalForFliers | AddressInformation                                        |
	| MyBoard  | True               | True                     | -37.7760:144.979:[][Brunswick East][VIC][3057][Australia] |
Then a BOARD named MyBoard will be created
And the BOARD will allow Others to post FLIERS
And the BOARD will require approval for posted FLIERS

Scenario: Participant Adds Flier To Board They Dont Own
Given there is a public board named publicBoard that requires approval
And I have created a FLIER
When I add the FLIER to the board 
Then The FLIER will be a member of the board with a status of PendingApproval 

Scenario: Participant Approves Flier On Board They Own
Given I have created a public board named coolBoard that requires approval
And A BROWSER has created a FLIER
And A BROWSER has added the FLIER to the board
When I approve the FLIER
Then The FLIER will be a member of the board with a status of Approved




