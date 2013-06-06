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

Scenario: Participant Adds Flier To Board They Dont Own
Given there is an approved public board named publicBoard
And I am a BROWSER in PARTICIPANT ROLE
And I have created a FLIER
When I add the FLIER to the board 
Then The FLIER will be a member of the board with a status of PendingApproval 

Scenario: Participant Approves Flier On Board They Own
Given I have created an approved public board named coolBoard 
And A BROWSER has created a FLIER
And A BROWSER has added the FLIER to the board
When I approve the FLIER
Then The FLIER will be a member of the board with a status of Approved

Scenario: When a Flier Approved on a Board is modified It requires re-approval
Given There is a FLIER that is Approved on a Board
When A BROWSER modifies the FLIER on a Board
Then The FLIER will be a member of the board with a status of PendingApproval 

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

Scenario: Create Flyer at Place Creates Venue Board if not exists
Given There is no Board for a Venue
And I am a PARTICIPANT with Admin ROLE
When I create a FLIER at a Venue
Then a Venue BOARD will be created
And The FLIER will be a member of the board with a status of Approved

Scenario: Create Flyer at Place Adds To Existing Venue Board
Given There is a Board for a Venue with a Flier
And I am a PARTICIPANT with Admin ROLE
When I create a FLIER at a Venue
Then The FLIER will be a member of the board with a status of Approved
And The Board will have 2 Fliers

Scenario: View Board
Given There is a Board for a Venue with a Flier
When I navigate to the public view page for that Board
Then I will see the Information for that Board
And I will see the Fliers on that Board



