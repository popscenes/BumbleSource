Feature: DefaultDynamicBulletinBoard
	As a BROWSER
	I want to view a DYNAMIC BULLETIN BOARD
	so that I can see FLIERS

Scenario: ViewDynamicBulletinFliers
	Given there are some TAGS set
	When I have navigated to the BULLETIN BOARD for a LOCATION
	Then I should only see FLIERS within a DISTANCE from that LOCATION
	And I should only see FLIERS with matching TAGS

Scenario: SetDynamicBulletinDistance
	Given there are some TAGS set 
	And I have navigated to the BULLETIN BOARD for a LOCATION
	When I set my DISTANCE 
	Then i should see all fliers within my new DISTANCE that have matching TAGS

Scenario: ViewDynamicBulletinFliersDefaultOrder
	Given there are some TAGS set 
	When I have navigated to the BULLETIN BOARD for a LOCATION
	Then I should see FLIERS ordered by create date

Scenario: View FLIER details
	Given There is a FLIER
	When I have navigated to the public view page for that FLIER
	Then I should see the public details of that FLIER

Scenario: SetDynamicBulletinDate
	Given there are some TAGS set 
	And I have navigated to the BULLETIN BOARD for a LOCATION
	When I set the event date filter
	Then I should only see FLIERS within a DISTANCE from that LOCATION
	And I should see only FLIERS with that event date


