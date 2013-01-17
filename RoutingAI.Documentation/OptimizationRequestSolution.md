# Optimizaton Request / Solution #

**Request** A set of resources and tasks to optimize

**Solution** The set of optimized routes

## Entity Details ##

**Resource** A resource (or group) available to complete tasks

Ex. 3 employees and 3 vehicles

Properties

- Origin
	- lat / lng
- (optional) Destination - where the resource will end
- Availability
	- 1/1/2012 9 am to 1/1/2012 5 pm
	- take into account the task time at the last visited Task and, if a destination is specified, the driving time to get there
- Skills
	- leaf collection
	- landscaping
	- tree removal
- Cost Per Mile
	- $0.50
- Cost Per Hour
	- $50 / hour

**Task** A task to complete for a reward

Ex. fix toilet

Properties

- Location
	- lat / lng
- Estimated Time - estimated time to complete the task
	- 30 minutes
- Time Confidence - affects buffer between tasks
	- 1-100%
- Value - the monetary reward for completing a task
	- $200
- (optional) Target Date & Range - ensures jobs happen on a user defined recurrence (every week/year/etc)
	- 1/1/2012 & 1 year
		- assigning the task for 1/2/2012 is perfectly fine, doing it on 11/1/2012 should be heavily penalized
	- penalize distance from target date exponentially, scaled by the target range
- Window Constraints - restricted date and times the task can be completed
	- 1/1/2012 3pm - 1/1/2012 5pm
	- 1/1/2012 11 pm - 1/2/2012 1 am
- Required Skill Type - optional
	- leaf collection

**Skill**

- Type
	- leaf collection
- Efficiency - affects estimated task time
	- 0-10, 0: Cannot complete. 10 = most efficient

**Route** a resource assigned to tasks in a specific order of time

- Resource
	- Only one assigned per route
- Destinations - tasks in order with their estimated arrivals / departures

## Future ##

These are future concepts that need revising.

**Resource**

- Capacities (if any filled must return to location before continuing skill)
	- volume, 3000 (gallons), 12414 English Garden Court, 47906 or .....
	- weight, 1000 (lbs)
- Overtime (if not defined: hard constraint)
	- 125% (first hour, employee specific, etc)
- Maximum distance before refueled

**Task**

- Capacity
	- 100 lbs
- Linked task - must be done consecutively or serially & or in the same group
- Dynamic price - script api for internal variables (ex. distance previous location)
- Dynamic value - worth more if performed by specific resources (higher rating resource group)

**Constraints** Time constraints, skill constraints

Hard constraints cannot charge price for a task if not met.  
Soft constraints charge a penalty cost (if resource group) or price (if task).