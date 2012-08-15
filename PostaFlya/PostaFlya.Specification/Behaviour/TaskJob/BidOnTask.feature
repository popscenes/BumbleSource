Feature: bidding on a task
As a PARTICIPANT with IdentityVerified role 
I want to be able to bid on a TASKJOB 
so that I am eligible for ASSIGNMENT to that TASKJOB


Scenario: Navigate To TaskJob Bid
Given there is a flier with TASKJOB behaviour 
Then I should be able to navigate to the BID page for that TASKJOB 

Scenario: TaskJob Bid
Given I have navigated to the BID page for a TASKJOB 
When I place a TASKBID on the TASKJOB 
Then the TASKBID will be registered against the TASKJOB
