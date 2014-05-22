using System;

namespace AASMAHoshimi
{
	public class CreateAgentAction : Action
	{
		public delegate void AgentCreatedDelegate(Type agentType);

		private AASMAAI ai;
		private Type agentType;
		private AgentCreatedDelegate onAgentCreated;

		public CreateAgentAction (AASMAAI ai, Type agentType) {
			this.ai = ai;
			this.agentType = agentType;
		}
		public CreateAgentAction (AASMAAI ai, Type agentType, AgentCreatedDelegate onAgentCreated)
		{
			this.ai = ai;
			this.agentType = agentType;
			this.onAgentCreated = onAgentCreated;
		}

		public void execute() {
			this.ai.getNanoBot().Build(agentType);
			if (onAgentCreated != null) {
				this.onAgentCreated (agentType);
			}
		}

		public void cancel() {
			// Nothing to do here
		}
	}
}

