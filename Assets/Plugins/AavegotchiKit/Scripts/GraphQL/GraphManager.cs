using Cysharp.Threading.Tasks;
using Nethereum.Unity.Rpc;
using PortalDefender.AavegotchiKit.Utils;
using SimpleGraphQL;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace PortalDefender.AavegotchiKit.GraphQL
{
    /**
     * This class is responsible for handling all queries to the Graph
     */
    public class GraphManager : SingletonBehaviour<GraphManager>
    {
        [SerializeField]
        GraphQLConfig coreGraphConfig;

        GraphQLClient coreGraphClient;

        [SerializeField]
        GraphQLConfig svgGraphConfig;

        GraphQLClient svgGraphClient;


        public override void Awake()
        {
            base.Awake();

            if (coreGraphConfig == null)
            {
                Debug.LogError("coreGraphConfig is not assigned!");
                return;
            }

            if (svgGraphConfig == null)
            {
                Debug.LogError("svgGraphConfig is not assigned!");
                return;
            }

            try
            {
                coreGraphClient = new GraphQLClient(coreGraphConfig);
                Debug.Log("Successfully created coreGraphClient: " + coreGraphClient);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error creating coreGraphClient: " + ex);
            }

            try
            {
                svgGraphClient = new GraphQLClient(svgGraphConfig);
                Debug.Log("Successfully created svgGraphClient: " + svgGraphClient);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error creating svgGraphClient: " + ex);
            }
        }

        public async UniTask<UserAccount> GetUserAccount(string userId, CancellationToken cancellationToken = default)
        {
            GetUserAavegotchis request = new GetUserAavegotchis(userId);
            GetUserAavegotchis.Result result = await request.Fetch(coreGraphClient, cancellationToken);
            if (result != null)
            {
                return result.User;
            }
            return null;
        }

        public async UniTask<GotchiData> GetGotchiData(string gotchiId, CancellationToken cancellationToken = default)
        {
            GetGotchiInfo request = new GetGotchiInfo(gotchiId);
            GetGotchiInfo.Result result = await request.Fetch(coreGraphClient, cancellationToken);
            if (result != null)
            {
                return result.Gotchi;
            }            
            return null;
        }


        //svg graph
        public async UniTask<GetGotchiSvg.GotchiSvgResult> GetGotchiSvg(string gotchiId, CancellationToken cancellationToken = default)
        {
            GetGotchiSvg request = new GetGotchiSvg(gotchiId);
            GetGotchiSvg.Result result = await request.Fetch(svgGraphClient, cancellationToken);
            if (result != null)
            {
                return result.GotchiSvg;
            }
            return null;
        }

        public async UniTask<List<GetGotchiSvgs.GotchiSvgResult>> GetGotchiSvgs(List<string> gotchiIds, CancellationToken cancellationToken = default)
        {
            GetGotchiSvgs request = new GetGotchiSvgs(gotchiIds);
            GetGotchiSvgs.Result result = await request.Fetch(svgGraphClient, cancellationToken);
            if (result != null)
            {
                return result.GotchiSvgs;
            }
            return null;
        }

    }
}
