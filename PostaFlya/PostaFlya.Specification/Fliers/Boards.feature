Feature: Boards
	In order for a range of people to see my flier
	As a participant
	I want to be able to add my flier to a range of boards


Scenario: Participant Creates Board
Given i am an existing BROWSER with PARTICIPANT ROLE
When I submit the following data for the BOARD:
	| BoardName | AcceptOthersFliers | RequireApprovalForFliers | AddressInformation                                        |
	| MyBoard  | True               | True                     | -37.7760:144.979:[][Brunswick East][VIC][3057][Australia] |
Then a private BOARD named MyBoard will be created

