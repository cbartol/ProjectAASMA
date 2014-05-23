using System;

namespace AASMAHoshimi
{
	public class CreateAgentAction : Action
	{
		public delegate void AgentCreatedDelegate(Type agentType);

		private AASMAAI ai;
		private Type agentType;
		private AgentCreatedDelegate onAgentCreated;
		private int id;

		public CreateAgentAction (AASMAAI ai, Type agentType, int id) {
			this.ai = ai;
			this.agentType = agentType;
			this.id = id;
		}
		public CreateAgentAction (AASMAAI ai, Type agentType, AgentCreatedDelegate onAgentCreated, int id)
		{
			this.ai = ai;
			this.agentType = agentType;
			this.id = id;
			this.onAgentCreated = onAgentCreated;
		}

		public void execute() {
			this.ai.getNanoBot().Build(agentType, agentType.ToString()[0] + id.ToString());
			if (onAgentCreated != null) {
				this.onAgentCreated (agentType);
			}
		}

		public void cancel() {
			// Nothing to do here
		}
	}
}

